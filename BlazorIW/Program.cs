using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlazorIW.Components;
using BlazorIW.Components.Account;
using BlazorIW.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlazorIW.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using BlazorIW.Client.Services;

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
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<LocalizationService>();

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


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorIW.Client._Imports).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
