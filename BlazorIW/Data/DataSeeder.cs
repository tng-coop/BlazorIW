using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;

using BlazorIW.Services;

namespace BlazorIW.Data;

public static class DataSeeder
{
    public static async Task SeedBackgroundVideosAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var client = scope.ServiceProvider.GetRequiredService<PexelsClient>();

        if (await db.BackgroundVideos.AnyAsync(cancellationToken))
            return;

        var waterfallInfo = await client.GetVideoInfoAsync(6394054, cancellationToken);
        var goatUrl = await client.GetVideoUrlAsync(30646036, cancellationToken);

        db.BackgroundVideos.Add(new BackgroundVideo
        {
            Name = "waterfall",
            Url = waterfallInfo.Url,
            Poster = waterfallInfo.Poster
        });

        db.BackgroundVideos.Add(new BackgroundVideo
        {
            Name = "goat",
            Url = goatUrl
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedHtmlContentsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.HtmlContents.AnyAsync(cancellationToken))
            return;

        db.HtmlContents.Add(new HtmlContentRevision
        {
            Revision = 1,
            Date = DateTime.UtcNow,
            Title = "Hello",
            Excerpt = "Welcome",
            Content = "<p>Hello, world!</p>",
            IsPublished = true
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedBranchOfficeContentsAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.BranchOfficeContents.AnyAsync(cancellationToken))
            return;
        var index = 1;
        foreach (var seed in BranchOfficeSeedData.Data)
        {
            for (var i = 0; i < seed.Count; i++)
            {
                db.BranchOfficeContents.Add(new BranchOfficeContent
                {
                    Revision = 1,
                    Date = DateTime.UtcNow,
                    BranchName = $"Branch {index}",
                    Address = $"Address {index}",
                    PostalCode = seed.PostalCode,
                    TelephoneNumber = "123-456-7890",
                    FaxNumber = "123-456-7891",
                    Email = $"branch{index}@example.com",
                    IsPublished = true
                });
                index++;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedPostalCodesAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await db.PostalCodes.AnyAsync(cancellationToken))
            return;

        db.PostalCodes.AddRange(PostalCodeSeedData.Data);
        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedDefaultUsersAsync(IServiceProvider services, string password, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "admin", "editor" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var users = new (string Email, string Role)[]
        {
            ("admin@example.com", "admin"),
            ("admin2@example.com", "admin"),
            ("editor@example.com", "editor"),
            ("editor2@example.com", "editor")
        };

        foreach (var (email, role) in users)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user {email}: {errors}");
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
