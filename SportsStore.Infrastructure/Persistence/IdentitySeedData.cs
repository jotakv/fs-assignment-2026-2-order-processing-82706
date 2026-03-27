using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SportsStore.Infrastructure.Persistence;

public static class IdentitySeedData
{
    private const string AdminUser = "Admin";
    private const string AdminPassword = "Secret123$";

    public static async Task EnsurePopulatedAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        AppIdentityDbContext context = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            await context.Database.MigrateAsync();
        }

        UserManager<IdentityUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        IdentityUser? user = await userManager.FindByNameAsync(AdminUser);

        if (user is not null)
        {
            return;
        }

        user = new IdentityUser(AdminUser)
        {
            Email = "admin@example.com",
            PhoneNumber = "555-1234"
        };

        await userManager.CreateAsync(user, AdminPassword);
    }
}
