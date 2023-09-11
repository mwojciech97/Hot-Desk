using HotDeskAPI.Controllers;
using HotDeskAPI.Models;
using HotDeskAPI.Tests;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Tests
{
    public class ReservationControllerTest : IClassFixture<TestDatabaseFixture>
    {
        public ReservationControllerTest(TestDatabaseFixture fixture) => Fixture = fixture;
        public TestDatabaseFixture Fixture { get; }
        [Fact]
        public async void CreateReservation_Test()
        {
            using var context = Fixture.CreateContext();
            var controller = new ReservationController(context);
            List<Reservation> reservations = new List<Reservation>();

            //Users
            User user1 = context.Users.FirstOrDefault(u => u.Username == "test");
            User user2 = context.Users.FirstOrDefault(u => u.Username == "admin");
            User user3 = context.Users.FirstOrDefault(u => u.Username == "test1");

            //Desks
            Desk desk1 = context.Desks.FirstOrDefault(d => d.Id == 1);
            Desk desk2 = context.Desks.FirstOrDefault(d => d.Id == 2);
            Desk desk3 = context.Desks.FirstOrDefault(d => d.Id == 3);

            //Location
            Location north = context.Locations.FirstOrDefault(l => l.LocationName == "North");
            Location south = context.Locations.FirstOrDefault(l => l.LocationName == "South");

            //Overlaping data with another reservation and the same desk
            reservations.Add(new Reservation());
            reservations[0].User = user2;
            reservations[0].Desk = desk1;
            reservations[0].Desk.Location = north;
            reservations[0].StartDate = DateTime.Now.AddDays(1).Date;
            reservations[0].EndDate = DateTime.Now.AddDays(4).Date;
            var task0 = controller.CreateReservation(reservations[0]);
            Assert.IsType<BadRequestObjectResult>(task0.Result);

            //Start date after end date
            reservations.Add(new Reservation());
            reservations[1].User = user2;
            reservations[1].Desk = desk3;
            reservations[1].Desk.Location = north;
            reservations[1].StartDate = DateTime.Now.AddDays(4).Date;
            reservations[1].EndDate = DateTime.Now.AddDays(2).Date;
            var task1 = controller.CreateReservation(reservations[1]);
            Assert.IsType<BadRequestObjectResult>(task1.Result);

            //Start date before today
            reservations.Add(new Reservation());
            reservations[2].User = user2;
            reservations[2].Desk = desk3;
            reservations[2].Desk.Location = north;
            reservations[2].StartDate = DateTime.Now.AddDays(-1).Date;
            reservations[2].EndDate = DateTime.Now.AddDays(1).Date;
            var task2 = controller.CreateReservation(reservations[2]);
            Assert.IsType<BadRequestObjectResult>(task2.Result);

            //Correct data with different desk and the same time
            reservations.Add(new Reservation());
            reservations[3].User = user2;
            reservations[3].Desk = desk3;
            reservations[3].Desk.Location = north;
            reservations[3].StartDate = DateTime.Now.AddDays(4).Date;
            reservations[3].EndDate = DateTime.Now.AddDays(5).Date;
            var task3 = controller.CreateReservation(reservations[3]);
            Assert.IsType<CreatedAtActionResult>(task3.Result);

            //Correct data with different desk and time
            reservations.Add(new Reservation());
            reservations[4].User = user2;
            reservations[4].Desk = desk3;
            reservations[4].Desk.Location = north;
            reservations[4].StartDate = DateTime.Now.AddDays(10).Date;
            reservations[4].EndDate = DateTime.Now.AddDays(12).Date;
            var task4 = controller.CreateReservation(reservations[4]);
            Assert.IsType<CreatedAtActionResult>(task4.Result);

            //Too long reservations in total
            reservations.Add(new Reservation());
            reservations[5].User = user2;
            reservations[5].Desk = desk3;
            reservations[5].Desk.Location = north;
            reservations[5].StartDate = DateTime.Now.AddDays(14).Date;
            reservations[5].EndDate = DateTime.Now.AddDays(16).Date;
            var task5 = controller.CreateReservation(reservations[5]);
            Assert.IsType<BadRequestObjectResult>(task5.Result);

            //Reservation of unavailable desk
            reservations.Add(new Reservation());
            reservations[6].User = user3;
            reservations[6].Desk = desk2;
            reservations[6].Desk.Location = north;
            reservations[6].StartDate = DateTime.Now.AddDays(20).Date;
            reservations[6].EndDate = DateTime.Now.AddDays(21).Date;
            var task6 = controller.CreateReservation(reservations[6]);
            Assert.IsType<BadRequestObjectResult>(task6.Result);

            //Too long one reservation 
            reservations.Add(new Reservation());
            reservations[7].User = user3;
            reservations[7].Desk = desk3;
            reservations[7].Desk.Location = north;
            reservations[7].StartDate = DateTime.Now.AddDays(20).Date;
            reservations[7].EndDate = DateTime.Now.AddDays(26).Date;
            var task7 = controller.CreateReservation(reservations[7]);
            Assert.IsType<BadRequestObjectResult>(task7.Result);

        }
    }
}
