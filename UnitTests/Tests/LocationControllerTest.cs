using HotDeskAPI.Controllers;
using HotDeskAPI.Models;
using HotDeskAPI.Tests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UnitTests.Tests
{
    public class LocationControllerTest : IClassFixture<TestDatabaseFixture>
    {
        public LocationControllerTest(TestDatabaseFixture fixture) => Fixture = fixture;
        public TestDatabaseFixture Fixture { get; }

        [Fact]
        public async void GetLocation()
        {
            using var context = Fixture.CreateContext();
            var controller = new LocationController(context);
            var resultTask = controller.Get();
            var result = await resultTask as OkObjectResult;
            Assert.NotNull(result);
            var locations = context.Locations.ToList();
            var resultLocations = result.Value as List<Location>;
            Assert.IsType<OkObjectResult>(result);
            for(int i = 0; i < locations.Count; i++)
            {
                Assert.Equal(locations[i], resultLocations[i]);
            }
        }
        [Fact]
        public async Task CreateLocation_Test()
        {
            string locationName = "testLocation";
            using var context = Fixture.CreateContext();
            var controller = new LocationController(context);
            var addedLocation = (Location)null;
            
            //Users
            var notAdmin = context.Users.Find(1);
            var notExisting = new User { Username = "testing", Password = "testing", IsAdmin = false };
            var admin = context.Users.Find(2);

            //Check if can create with non admin user
            var createResult = await controller.CreateLocation(locationName, notAdmin);
            var badRequestStatus = (BadRequestObjectResult)createResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);
            addedLocation = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.Null(addedLocation);

            //Check if can create with non existing user
            createResult = await controller.CreateLocation(locationName, notExisting);
            badRequestStatus = (BadRequestObjectResult)createResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);
            addedLocation = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.Null(addedLocation);

            //Check if can create with admin user
            createResult = await controller.CreateLocation(locationName, admin);
            var createdResult = (CreatedAtActionResult)createResult;
            Assert.IsType<CreatedAtActionResult>(createdResult);
            Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
            addedLocation = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.NotNull(addedLocation);
            Assert.Equal(addedLocation.LocationName, locationName);

            //Check if can create with admin user and existing location
            createResult = await controller.CreateLocation("North", admin);
            badRequestStatus = (BadRequestObjectResult)createResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);

        }
        [Fact]
        public async Task Delete_Test()
        {
            string locationName = "South";
            using var context = Fixture.CreateContext();
            var controller = new LocationController(context);
            //Users
            var notAdmin = context.Users.Find(1);
            var notExisting = new User { Username = "testing", Password = "testing", IsAdmin = false };
            var admin = context.Users.Find(2);
            
            //Check if can delete with non admin user
            var deleteResult = await controller.Delete(locationName, notAdmin);
            var badRequestStatus = (BadRequestObjectResult)deleteResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);
            var checkNotAdmin = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.NotNull(checkNotAdmin);

            //Check if can delete with non existing user
            deleteResult = await controller.Delete(locationName, notExisting);
            badRequestStatus = (BadRequestObjectResult)deleteResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);
            var checkNotExisting = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.NotNull(checkNotExisting);
            
            //Check if can delete with admin user
            deleteResult = await controller.Delete(locationName, admin);
            var noContentStatus = (NoContentResult)deleteResult;
            Assert.IsType<NoContentResult>(noContentStatus);
            Assert.Equal(StatusCodes.Status204NoContent, noContentStatus.StatusCode);
            var checkAdmin = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.Null(checkAdmin);
            
            //Check if can delete with desks in location
            locationName = "North";
            deleteResult = await controller.Delete(locationName, admin);
            badRequestStatus = (BadRequestObjectResult)deleteResult;
            Assert.IsType<BadRequestObjectResult>(badRequestStatus);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestStatus.StatusCode);
            var checkIfNorthExists = context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            Assert.NotNull(checkIfNorthExists);
        }
    }
}
