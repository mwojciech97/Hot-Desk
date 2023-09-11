using HotDeskAPI.DataContext;
using HotDeskAPI.Model;
using HotDeskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace HotDeskAPI.Tests
{
    public class TestDatabaseFixture
    {
        private const string ConnectionString = @"Server=Maks;Database=HotDesk;Trusted_Connection=True;TrustServerCertificate=True;";
        private static readonly object _lock = new();
        private static bool _databaseInitialized;

        public TestDatabaseFixture()
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using (var context = CreateContext())
                    {
                        context.Database.EnsureDeleted();
                        context.Database.EnsureCreated();

                        context.AddRange(
                            new Location { LocationName = "North" },
                            new Location { LocationName = "South" });
                        context.SaveChanges();
                        context.AddRange(
                            new Desk { DeskId = 1, LocationId = 1, IsAvailable = true },
                            new Desk { DeskId = 2, LocationId = 1, IsAvailable = false },
                            new Desk { DeskId = 3, LocationId = 1, IsAvailable = true });
                        context.SaveChanges();
                        context.AddRange(
                            new User { Username = "test", Password = "test", IsAdmin = false },
                            new User { Username = "admin", Password = "admin", IsAdmin = true },
                            new User { Username = "test1", Password = "test1", IsAdmin = false });
                        context.SaveChanges();
                        context.AddRange(
                            new Employee { Name = "tester", Surrname = "test", UserId = 1 },
                            new Employee { Name = "administrator", Surrname = "admin", UserId = 2 },
                            new Employee { Name = "tester1", Surrname = "test1", UserId = 3 });
                        context.SaveChanges();
                        context.AddRange(
                            new Reservation { User = context.Users.FirstOrDefault(u => u.Username == "test"),
                                            Desk = context.Desks.FirstOrDefault(d => d.Id == 1),
                                            StartDate = DateTime.Now.AddDays(2),
                                            EndDate = DateTime.Now.AddDays(3)
                            },
                            new Reservation
                            {
                                User = context.Users.FirstOrDefault(u => u.Username == "test"),
                                Desk = context.Desks.FirstOrDefault(d => d.Id == 1),
                                StartDate = DateTime.Now.AddDays(4),
                                EndDate = DateTime.Now.AddDays(5)
                            });
                        context.SaveChanges();
                    }
                    _databaseInitialized = true;
                }
            }
        }
        public HotDeskDBContext CreateContext() =>
            new HotDeskDBContext(
                new DbContextOptionsBuilder<HotDeskDBContext>()
                .UseSqlServer(ConnectionString)
                .Options);
    }
}
