using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UserPermissions.Domain.Entities;
using UserPermissions.Infrastructure.Persistence;
using UserPermissions.Infrastructure.Repositories;
using Xunit;

namespace UserPermissions.Tests.Infrastructure.Repositories
{
    public class RoleRepositoryTests
    {
        private static AppDbContext BuildContext()
        {
            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(opts);
        }

        [Fact]
        public async Task GetByNameAsync_Returns_Role_When_Exists()
        {
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            var admin = new Role { Name = "admin" };
            db.Roles.Add(admin);
            await db.SaveChangesAsync();

            var found = await repo.GetByNameAsync("admin", CancellationToken.None);

            found.Should().NotBeNull();
            found!.Name.Should().Be("admin");
        }

        [Fact]
        public async Task GetByNameAsync_Returns_Null_When_Not_Exists()
        {
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            var found = await repo.GetByNameAsync("nope", CancellationToken.None);

            found.Should().BeNull();
        }

        [Fact]
        public async Task GetOrCreateAsync_Creates_And_Persists_When_Missing()
        {
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            var created = await repo.GetOrCreateAsync("  manager  ", CancellationToken.None);

            created.Should().NotBeNull();
            created.Name.Should().Be("manager"); // trimmed
            (await db.Roles.CountAsync()).Should().Be(1);

            // Confirm persisted in database and retrievable
            var reload = await db.Roles.FirstOrDefaultAsync(r => r.Name == "manager");
            reload.Should().NotBeNull();
        }

        [Fact]
        public async Task GetOrCreateAsync_Returns_Existing_Without_Duplicating()
        {
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            var existing = db.Roles.Add(new Role { Name = "editor" }).Entity;
            await db.SaveChangesAsync();

            var returned = await repo.GetOrCreateAsync("editor", CancellationToken.None);

            // Same tracked instance in the same DbContext
            returned.Should().BeSameAs(existing);
            (await db.Roles.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task GetOrCreateAsync_Trims_Input_And_Avoids_Duplicate_When_Spaced()
        {
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            var existing = db.Roles.Add(new Role { Name = "admin" }).Entity;
            await db.SaveChangesAsync();

            var returned = await repo.GetOrCreateAsync("   admin   ", CancellationToken.None);

            // Should return the existing tracked entity (idempotent)
            returned.Should().BeSameAs(existing);
            (await db.Roles.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task Case_Sensitivity_Demonstration_GetOrCreate_Creates_Distinct_Role_For_Different_Case()
        {
            // Current implementation only Trims; it does NOT ToLowerInvariant().
            // So "Admin" and "admin" are treated as different names.
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            db.Roles.Add(new Role { Name = "admin" });
            await db.SaveChangesAsync();

            var created = await repo.GetOrCreateAsync("Admin", CancellationToken.None);

            created.Name.Should().Be("Admin");
            (await db.Roles.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task GetByNameAsync_Does_Not_Trim_Input()
        {
            // By design, GetByNameAsync compares exact Name; it does not Trim the input.
            using var db = BuildContext();
            var repo = new RoleRepository(db);

            db.Roles.Add(new Role { Name = "user" });
            await db.SaveChangesAsync();

            var notFound = await repo.GetByNameAsync("  user  ", CancellationToken.None);
            notFound.Should().BeNull();

            var found = await repo.GetByNameAsync("user", CancellationToken.None);
            found.Should().NotBeNull();
        }
    }
}
