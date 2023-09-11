using HotDeskAPI.DataContext;
using HotDeskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly HotDeskDBContext _context;
        public LocationController(HotDeskDBContext context) => _context = context;

        [HttpGet("locations")]
        public async Task<IActionResult> Get()
        {
            var locations = await _context.Locations.ToListAsync();
            return locations == null ? NotFound() : Ok(locations);
        }
        [HttpPost("createlocation")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateLocation(string locationName, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest("You do not have permission to add new location.");
                if (_context.Locations.Any(l => l.LocationName == locationName)) return BadRequest("Location already exists.");
                Location newLocation = new Location();
                newLocation.LocationName = locationName;
                await _context.Locations.AddAsync(newLocation);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = newLocation.Id }, newLocation);
            } catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            
        }
        [HttpDelete("deletelocation/{locationName}")]
        public async Task<IActionResult> Delete(string locationName, User user)
        {
            var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (u == null ||
                u.Password != user.Password ||
                !u.IsAdmin) return BadRequest("Wrong username or password.");
            var deleteLocation =  _context.Locations.FirstOrDefault(l => l.LocationName == locationName);
            if (deleteLocation == null) return NotFound();
            if (_context.Desks.Any(d => d.Location.LocationName == locationName)) return BadRequest("Cannot delete location as there are still desks in there.");
            _context.Remove(deleteLocation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
