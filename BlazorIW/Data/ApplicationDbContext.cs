using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorIW.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<BackgroundVideo> BackgroundVideos => Set<BackgroundVideo>();
    public DbSet<HtmlContentRevision> HtmlContents => Set<HtmlContentRevision>();
    public DbSet<BranchOfficeContent> BranchOfficeContents => Set<BranchOfficeContent>();
    public DbSet<PostalCode> PostalCodes => Set<PostalCode>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<HtmlContentRevision>().HasKey(h => new { h.Id, h.Revision });
        builder.Entity<HtmlContentRevision>()
            .HasIndex(h => new { h.Id, h.IsReviewRequested })
            .IsUnique()
            .HasFilter("\"IsReviewRequested\" = TRUE");
        builder.Entity<HtmlContentRevision>()
            .HasIndex(h => new { h.Id, h.IsPublished })
            .IsUnique()
            .HasFilter("\"IsPublished\" = TRUE");

        builder.Entity<BranchOfficeContent>().HasKey(b => new { b.Id, b.Revision });
        builder.Entity<BranchOfficeContent>()
            .HasIndex(b => new { b.Id, b.IsReviewRequested })
            .IsUnique()
            .HasFilter("\"IsReviewRequested\" = TRUE");
        builder.Entity<BranchOfficeContent>()
            .HasIndex(b => new { b.Id, b.IsPublished })
            .IsUnique()
            .HasFilter("\"IsPublished\" = TRUE");

        builder.Entity<PostalCode>().HasKey(p => p.Zipcode);
    }
}
