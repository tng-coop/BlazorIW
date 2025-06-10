using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BlazorIW.Data;
using Xunit;
using System.Threading.Tasks;

namespace BlazorIW.Tests;

public class HtmlContentWorkflowTests
{
    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public void Adding_two_published_revisions_fails()
    {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var id = Guid.NewGuid();
        context.HtmlContents.Add(new HtmlContentRevision
        {
            Id = id,
            Revision = 1,
            Html = "A",
            IsPublished = true
        });
        context.SaveChanges();

        context.HtmlContents.Add(new HtmlContentRevision
        {
            Id = id,
            Revision = 2,
            Html = "B",
            IsPublished = true
        });

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public void Review_request_flag_is_unique_per_content()
    {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var id = Guid.NewGuid();
        context.HtmlContents.Add(new HtmlContentRevision { Id = id, Revision = 1, Html = "R1" });
        context.SaveChanges();

        var rev2 = new HtmlContentRevision { Id = id, Revision = 2, Html = "R2", IsReviewRequested = true };
        context.HtmlContents.Add(rev2);
        context.SaveChanges();

        context.HtmlContents.Add(new HtmlContentRevision { Id = id, Revision = 3, Html = "R3", IsReviewRequested = true });

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public async Task Can_request_review_after_clearing_previous()
    {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var id = Guid.NewGuid();
        context.HtmlContents.Add(new HtmlContentRevision { Id = id, Revision = 1, Html = "R1" });
        var rev2 = new HtmlContentRevision { Id = id, Revision = 2, Html = "R2", IsReviewRequested = true };
        context.HtmlContents.Add(rev2);
        context.SaveChanges();

        rev2.IsReviewRequested = false;
        context.SaveChanges();

        context.HtmlContents.Add(new HtmlContentRevision { Id = id, Revision = 3, Html = "R3", IsReviewRequested = true });
        context.SaveChanges();

        Assert.Equal(3, await context.HtmlContents.CountAsync());
    }
}
