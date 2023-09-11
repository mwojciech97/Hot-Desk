using HotDeskAPI.Controllers;
using HotDeskAPI.Models;
using HotDeskAPI.Tests;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Tests
{
    public class DeskControllerTest : IClassFixture<TestDatabaseFixture>
    {
        public DeskControllerTest(TestDatabaseFixture fixture) => Fixture = fixture;
        public TestDatabaseFixture Fixture { get; }

        [Fact]
        public async void Get()
        {
            using var context = Fixture.CreateContext();
            var controller = new DeskController(context);
            var resultTask = controller.Get();
            var result = await resultTask as OkObjectResult;
            Assert.NotNull(result);
            var desks = context.Desks.ToList();
            var resultDesks = result.Value as List<Desk>;
            Assert.IsType<OkObjectResult>(result);
            for(int i = 0; i < desks.Count; i++)
            {
                Assert.Equal(desks[i], resultDesks[i]);
                Assert.NotNull(resultDesks[i].Location);
            }
        }
        [Fact]
        public async Task GetAvailable_Test()
        {
            using var context = Fixture.CreateContext();
            var controller = new DeskController(context);
            //Check with correct data (from at least the next day)
            var resultTask = controller.GetAvailable(DateTime.Now.AddDays(1).ToString(), DateTime.Now.AddDays(2).ToString());
            var result = await resultTask as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            //Check with start date set to today
            resultTask = controller.GetAvailable(DateTime.Now.ToString(), DateTime.Now.AddDays(2).ToString());
            Assert.IsType<BadRequestObjectResult>(resultTask.Result);

            //Check with end date before start date
            resultTask = controller.GetAvailable(DateTime.Now.ToString(), DateTime.Now.AddDays(-2).ToString());
            Assert.IsType<BadRequestObjectResult>(resultTask.Result);

            //Check with start date before today
            resultTask = controller.GetAvailable(DateTime.Now.AddDays(-2).ToString(), DateTime.Now.ToString());
            Assert.IsType<BadRequestObjectResult>(resultTask.Result);
        }
        [Fact]
        public async Task GetDesksInLocation_Test()
        {
            using var context = Fixture.CreateContext();
            var controller = new DeskController(context);
            //Check with desks in location
            var resultTask = controller.GetDesksInLocation("North");
            var result = await resultTask as OkObjectResult;
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            //Check without desks in location
            resultTask = controller.GetDesksInLocation("South");
            Assert.IsType<NotFoundObjectResult>(resultTask.Result);
        }
    }
}
