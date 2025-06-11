using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BlazorIW.Data;
using Xunit;
using System.Threading.Tasks;

namespace BlazorIW.Tests;

public class BranchOfficeContentWorkflowTests
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
        context.BranchOfficeContents.Add(new BranchOfficeContent
        {
            Id = id,
            Revision = 1,
            Date = DateTime.UtcNow,
            BranchName = "A",
            Address = "A",
            PostalCode = "1",
            TelephoneNumber = "1",
            FaxNumber = "1",
            Email = "a@example.com",
            IsPublished = true
        });
        context.SaveChanges();

        context.BranchOfficeContents.Add(new BranchOfficeContent
        {
            Id = id,
            Revision = 2,
            Date = DateTime.UtcNow,
            BranchName = "B",
            Address = "B",
            PostalCode = "2",
            TelephoneNumber = "2",
            FaxNumber = "2",
            Email = "b@example.com",
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
        context.BranchOfficeContents.Add(new BranchOfficeContent { Id = id, Revision = 1, Date = DateTime.UtcNow, BranchName = "R1", Address = "R1", PostalCode = "1", TelephoneNumber = "1", FaxNumber = "1", Email = "r1@example.com" });
        context.SaveChanges();

        var rev2 = new BranchOfficeContent { Id = id, Revision = 2, Date = DateTime.UtcNow, BranchName = "R2", Address = "R2", PostalCode = "2", TelephoneNumber = "2", FaxNumber = "2", Email = "r2@example.com", IsReviewRequested = true };
        context.BranchOfficeContents.Add(rev2);
        context.SaveChanges();

        context.BranchOfficeContents.Add(new BranchOfficeContent { Id = id, Revision = 3, Date = DateTime.UtcNow, BranchName = "R3", Address = "R3", PostalCode = "3", TelephoneNumber = "3", FaxNumber = "3", Email = "r3@example.com", IsReviewRequested = true });

        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
    }

    [Fact]
    public async Task Can_request_review_after_clearing_previous()
    {
        using var connection = CreateConnection();
        using var context = CreateContext(connection);

        var id = Guid.NewGuid();
        context.BranchOfficeContents.Add(new BranchOfficeContent { Id = id, Revision = 1, Date = DateTime.UtcNow, BranchName = "R1", Address = "R1", PostalCode = "1", TelephoneNumber = "1", FaxNumber = "1", Email = "r1@example.com" });
        var rev2 = new BranchOfficeContent { Id = id, Revision = 2, Date = DateTime.UtcNow, BranchName = "R2", Address = "R2", PostalCode = "2", TelephoneNumber = "2", FaxNumber = "2", Email = "r2@example.com", IsReviewRequested = true };
        context.BranchOfficeContents.Add(rev2);
        context.SaveChanges();

        rev2.IsReviewRequested = false;
        context.SaveChanges();

        context.BranchOfficeContents.Add(new BranchOfficeContent { Id = id, Revision = 3, Date = DateTime.UtcNow, BranchName = "R3", Address = "R3", PostalCode = "3", TelephoneNumber = "3", FaxNumber = "3", Email = "r3@example.com", IsReviewRequested = true });
        context.SaveChanges();

        Assert.Equal(3, await context.BranchOfficeContents.CountAsync());
    }
}
