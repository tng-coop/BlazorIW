using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorIW.Components;
using BlazorIW.Components.Account;
using BlazorIW.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlazorIW.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Mvc;
using BlazorIW.Client.Services;

const int WaterfallVideoId = 6394054;
const int GoatVideoId = 30646036;
var isDatabaseAvailable = true;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityEmailSender>();
builder.Services.AddHttpClient<PexelsClient>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<WebRootFileService>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<BrowserStorageService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<WordPressService>();
builder.Services.AddScoped<HtmlContentService>();

var app = builder.Build();

var defaultUserPassword = builder.Configuration["DefaultUser:Password"];
if (string.IsNullOrWhiteSpace(defaultUserPassword))
{
    throw new InvalidOperationException("DefaultUser__Password environment variable must be set.");
}


// Attempt to initialize the database; continue even if it fails
using (var scope = app.Services.CreateScope())
{
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.EnsureCreated();
        DataSeeder.SeedBackgroundVideosAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        DataSeeder.SeedHtmlContentsAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        DataSeeder.SeedBranchOfficeContentsAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        DataSeeder.SeedDefaultUsersAsync(scope.ServiceProvider, defaultUserPassword).GetAwaiter().GetResult();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve files from wwwroot for environments where MapStaticAssets might not
// register the middleware correctly (e.g., certain container hosts).
app.UseStaticFiles();

app.UseAntiforgery();

app.MapStaticAssets();



app.MapGet("/api/waterfall-video-info", async (ApplicationDbContext db, PexelsClient client, CancellationToken ct) =>
{
    if (isDatabaseAvailable)
    {
        try
        {
            var info = await db.BackgroundVideos.FirstOrDefaultAsync(v => v.Name == "waterfall", ct);
            if (info is not null)
            {
                return Results.Json(new { url = info.Url, poster = info.Poster });
            }
        }
        catch
        {
            // fall back to Pexels
        }
    }

    var videoInfo = await client.GetVideoInfoAsync(WaterfallVideoId, ct);
    return Results.Json(new { url = videoInfo.Url, poster = videoInfo.Poster });
});

app.MapGet("/api/waterfall-video-url", async (ApplicationDbContext db, PexelsClient client, CancellationToken ct) =>
{
    if (isDatabaseAvailable)
    {
        try
        {
            var info = await db.BackgroundVideos.FirstOrDefaultAsync(v => v.Name == "waterfall", ct);
            if (info is not null)
            {
                return Results.Json(new { url = info.Url });
            }
        }
        catch
        {
            // fall back to Pexels
        }
    }

    var url = await client.GetVideoUrlAsync(WaterfallVideoId, ct);
    return Results.Json(new { url });
});

app.MapGet("/api/goat-video-url", async (ApplicationDbContext db, PexelsClient client, CancellationToken ct) =>
{
    if (isDatabaseAvailable)
    {
        try
        {
            var info = await db.BackgroundVideos.FirstOrDefaultAsync(v => v.Name == "goat", ct);
            if (info is not null)
            {
                return Results.Json(new { url = info.Url });
            }
        }
        catch
        {
            // fall back to Pexels
        }
    }

    var url = await client.GetVideoUrlAsync(GoatVideoId, ct);
    return Results.Json(new { url });
});

app.MapGet("/api/ef-model", async (ApplicationDbContext db) =>
{
    var entityModels = new List<object>();

    foreach (var e in db.Model.GetEntityTypes())
    {
        var properties = e.GetProperties()
            .Select(p => new { Name = p.Name, Type = p.ClrType.Name })
            .ToList();
        var navigations = e.GetNavigations()
            .Select(n => new { Name = n.Name, Target = n.TargetEntityType.ClrType.Name })
            .ToList();

        var rows = new List<Dictionary<string, string?>>();
        try
        {
            var rowObjects = await db.GetQueryable(e.ClrType).Take(5).ToListAsync();
            foreach (var obj in rowObjects)
            {
                var dict = new Dictionary<string, string?>();
                foreach (var prop in e.GetProperties())
                {
                    var value = prop.PropertyInfo?.GetValue(obj);
                    dict[prop.Name] = value?.ToString();
                }
                rows.Add(dict);
            }
        }
        catch
        {
            // ignore errors when fetching sample data
        }

        entityModels.Add(new
        {
            Name = e.ClrType.Name,
            Properties = properties,
            Navigations = navigations,
            Rows = rows
        });
    }

    return Results.Json(entityModels);
});

app.MapPost("/api/upload-test", async (HttpContext context) =>
{
    long count = 0;
    var buffer = new byte[16 * 1024];
    int read;
    while ((read = await context.Request.Body.ReadAsync(buffer, context.RequestAborted)) > 0)
    {
        count += read;
    }
    return Results.Json(new { received = count });
}).DisableAntiforgery();




app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorIW.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapGet("/api/html-content-titles", async (ApplicationDbContext db, CancellationToken ct) =>
{
    var titles = await db.HtmlContents
        .Select(h => h.Title)
        .Distinct()
        .ToListAsync(ct);
    return Results.Json(titles);
});

app.MapGet("/api/html-contents", async (ApplicationDbContext db, CancellationToken ct) =>
{
    var items = await db.HtmlContents
        .Select(h => new HtmlContentDto(h.Id, h.Revision, h.Date, h.Title, h.Excerpt, h.Content, h.IsReviewRequested, h.IsPublished))
        .ToListAsync(ct);
    return Results.Json(items);
});

app.MapPost("/api/html-content-status", async (ApplicationDbContext db, UpdateStatusDto dto, CancellationToken ct) =>
{
    var item = await db.HtmlContents.FirstOrDefaultAsync(h => h.Id == dto.Id && h.Revision == dto.Revision, ct);
    if (item is null)
    {
        return Results.NotFound();
    }

    switch (dto.Status)
    {
        case "Published":
            var currentPublished = await db.HtmlContents.FirstOrDefaultAsync(h => h.Id == dto.Id && h.IsPublished, ct);
            if (currentPublished is not null && currentPublished != item)
            {
                currentPublished.IsPublished = false;
            }
            item.IsPublished = true;
            item.IsReviewRequested = false;
            break;
        case "Review":
            var currentReview = await db.HtmlContents.FirstOrDefaultAsync(h => h.Id == dto.Id && h.IsReviewRequested, ct);
            if (currentReview is not null && currentReview != item)
            {
                currentReview.IsReviewRequested = false;
            }
            item.IsReviewRequested = true;
            item.IsPublished = false;
            break;
        default:
            item.IsPublished = false;
            item.IsReviewRequested = false;
            break;
    }

    await db.SaveChangesAsync(ct);
    return Results.Ok();
}).RequireAuthorization(policy => policy.RequireRole("admin"));

app.MapPost("/api/import-html-content", async (ILogger<Program> logger, ApplicationDbContext db, [FromBody] List<ImportPostDto> posts, CancellationToken ct) =>
{
    logger.LogInformation("Received import request for {Count} posts", posts.Count);
    foreach (var post in posts)
    {
        logger.LogInformation("Received post titled '{Title}'", post.Title);
    }
    var existing = await db.HtmlContents.Select(h => h.Title).ToListAsync(ct);
    var added = 0;
    foreach (var p in posts)
    {
        if (existing.Contains(p.Title))
        {
            logger.LogInformation("Skipping existing post with title '{Title}'", p.Title);
            continue;
        }

        var date = DateTimeOffset.TryParse(p.Date, out var dto)
            ? dto.UtcDateTime
            : DateTime.UtcNow;
        db.HtmlContents.Add(new HtmlContentRevision
        {
            Id = Guid.NewGuid(),
            Revision = 1,
            Date = date,
            Title = p.Title,
            Excerpt = p.Excerpt,
            Content = p.Content,
            IsPublished = true
        });
        existing.Add(p.Title);
        added++;
    }

    if (added > 0)
    {
        logger.LogInformation("Saving {Count} new posts", added);
        await db.SaveChangesAsync(ct);
    }

    logger.LogInformation("Import complete. Added {Count} posts", added);
    return Results.Json(new { added });
}).DisableAntiforgery();

app.MapGet("/api/files", (WebRootFileService service) => Results.Json(service.GetFiles().ToList()));

app.MapGet("/api/users", async (UserManager<ApplicationUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    var list = new List<UserInfo>();
    foreach (var u in users)
    {
        var roles = await userManager.GetRolesAsync(u);
        var disabled = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow;
        list.Add(new UserInfo(u.Id, u.Email ?? string.Empty, roles.ToList(), disabled));
    }
    return Results.Json(list);
}).RequireAuthorization(p => p.RequireRole("admin"));

app.MapPost("/api/users", async (CreateUserDto dto, UserManager<ApplicationUser> userManager) =>
{
    var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
    var result = await userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(result.Errors.Select(e => e.Description));
    }
    await userManager.AddToRoleAsync(user, dto.Role);
    return Results.Ok();
}).RequireAuthorization(p => p.RequireRole("admin"));

app.MapPost("/api/users/{id}/role", async (ClaimsPrincipal caller, string id, SetRoleDto dto, UserManager<ApplicationUser> userManager) =>
{
    var currentId = userManager.GetUserId(caller);
    if (currentId == id)
    {
        return Results.BadRequest("Cannot change your own role");
    }
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }
    var roles = await userManager.GetRolesAsync(user);
    await userManager.RemoveFromRolesAsync(user, roles);
    await userManager.AddToRoleAsync(user, dto.Role);
    return Results.Ok();
}).RequireAuthorization(p => p.RequireRole("admin"));

app.MapPost("/api/users/{id}/disable", async (ClaimsPrincipal caller, string id, SetDisabledDto dto, UserManager<ApplicationUser> userManager) =>
{
    var currentId = userManager.GetUserId(caller);
    if (currentId == id)
    {
        return Results.BadRequest("Cannot disable yourself");
    }
    var user = await userManager.FindByIdAsync(id);
    if (user is null)
    {
        return Results.NotFound();
    }
    await userManager.SetLockoutEnabledAsync(user, dto.Disabled);
    await userManager.SetLockoutEndDateAsync(user, dto.Disabled ? DateTimeOffset.MaxValue : null);
    return Results.Ok();
}).RequireAuthorization(p => p.RequireRole("admin"));

app.Run();

record ImportPostDto(string Date, string Title, string Excerpt, string Content);
record HtmlContentDto(Guid Id, int Revision, DateTime Date, string Title, string Excerpt, string Content, bool IsReviewRequested, bool IsPublished);
record UpdateStatusDto(Guid Id, int Revision, string Status);
record UserInfo(string Id, string Email, List<string> Roles, bool IsDisabled);
record CreateUserDto(string Email, string Password, string Role);
record SetRoleDto(string Role);
record SetDisabledDto(bool Disabled);

