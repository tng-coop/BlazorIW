using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlazorIW.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<BackgroundVideo> BackgroundVideos => Set<BackgroundVideo>();
    public DbSet<HtmlContentRevision> HtmlContents => Set<HtmlContentRevision>();
    public DbSet<BranchOfficeContent> BranchOfficeContents => Set<BranchOfficeContent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<HtmlContentRevision>().HasKey(h => new { h.Id, h.Revision });
    }
}
