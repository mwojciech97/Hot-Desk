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
        [ProducesResponseType(typeof(Location), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var locations = await _context.Locations.ToListAsync();
                return locations == null ? NotFound() : Ok(locations);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(string locationName, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest("Wrong username or password.");
                var deleteLocation = _context.Locations.FirstOrDefault(l => l.LocationName == locationName);
                if (deleteLocation == null) return NotFound();
                if (_context.Desks.Any(d => d.Location.LocationName == locationName)) return BadRequest("Cannot delete location as there are still desks in there.");
                _context.Remove(deleteLocation);
                await _context.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
            
        }

    }
}
