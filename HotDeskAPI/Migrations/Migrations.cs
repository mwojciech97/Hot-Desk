using HotDeskAPI.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Hosting;

namespace HotDeskAPI.Migrations
{
    public static class Migrations
    {
        public static void MigrateDatabase(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<HotDeskDBContext>();
                    context.Database.Migrate();
                }
                catch (Exception e)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occurred while migrating the database.");
                }
            }
        }
    }
}
