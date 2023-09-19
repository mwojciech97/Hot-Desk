using HotDeskAPI.DataContext;
using HotDeskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeskController : ControllerBase
    {
        private readonly HotDeskDBContext _context;
        public DeskController(HotDeskDBContext context) => _context = context;

        [HttpGet("desks")]
        [ProducesResponseType(typeof(Desk), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var desks = await _context.Desks.Include(l => l.Location).ToListAsync();
                return desks.Count == 0 ? NotFound("No desks where found.") : Ok(desks);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet("availabledesks")]
        [ProducesResponseType(typeof(Desk), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailable(string Start, string End)
        {
            try
            {
                DateTime StartDate, EndDate;
                if (!DateTime.TryParse(Start, out StartDate) ||
                    !DateTime.TryParse(End, out EndDate)) return BadRequest("Invalid data format.");
                if (StartDate > EndDate ||
                    StartDate.Date <= DateTime.Now.Date) return BadRequest("Invalid data provided.");
                var desks = await _context.Desks.Include(d => d.Location).ToListAsync();
                var availabledesks = desks.Where(d => d.IsAvailable).ToList();

                var reserveddesks = await _context.Reservations.Where(d => desks.Contains(d.Desk) &&
                                                                        (d.StartDate.Date <= EndDate.Date &&
                                                                        d.EndDate.Date >= StartDate.Date) ||
                                                                        (StartDate.Date <= d.StartDate.Date &&
                                                                        EndDate.Date >= d.EndDate.Date))
                                                                .ToListAsync();

                var result = availabledesks.Except(reserveddesks.Select(r => r.Desk)).ToList();
                return result.Count == 0 ? NotFound("No desks available for reservation.") : Ok(result);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPost("desksreservation")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDeskReservation(string deskLocation, int deskId, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                var deskReserved = await _context.Reservations.Include(u => u.User)
                                                                .Include(d => d.Desk)
                                                                .Include(l => l.Desk.Location)
                                                                .Where(d => d.Desk.DeskId == deskId &&
                                                                       d.Desk.Location.LocationName == deskLocation &&
                                                                       d.StartDate.Date > DateTime.Now.Date ||
                                                                       d.EndDate.Date > DateTime.Now.Date)
                                                                .ToListAsync();
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest("Wrong username or password.");
                return deskReserved.Count == 0 ? NotFound("Desk not reserved.") : Ok(deskReserved);
            } catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet("getdesksinlocation/{deskLocation}")]
        [ProducesResponseType(typeof(Desk), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDesksInLocation(string deskLocation)
        {
            try
            {
                var desks = await _context.Desks.Include(l => l.Location).Where(d => d.Location.LocationName == deskLocation).ToListAsync();
                return desks.Count == 0 ? NotFound("No desks were found in provided location.") : Ok(desks);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPost("createdesk/{deskLocation}/{deskId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDesk(User user, string deskLocation, int deskId, bool isAvailable = true)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest("You do not have permission to add new desk.");

                var l = _context.Locations.FirstOrDefault(l => l.LocationName == deskLocation);
                if (l == null) return NotFound("Location not found!");

                Desk NewDesk = new Desk();
                NewDesk.LocationId = l.Id;
                NewDesk.DeskId = deskId;
                NewDesk.IsAvailable = isAvailable;
                bool deskExists = _context.Desks.Any(d => d.DeskId == NewDesk.DeskId && d.LocationId == l.Id);
                if (deskExists) return BadRequest("Desk already exist.");
                await _context.Desks.AddAsync(NewDesk);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Get), new { id = NewDesk.Id }, NewDesk);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
        [HttpPost("deskavailability/{deskLocation}/{deskId}/{IsAvailable}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeDeskAvailabilty(User user, string deskLocation, int deskId, bool IsAvailable)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest("Wrong username or password.");
                var desk = await _context.Desks.FirstOrDefaultAsync(d => d.DeskId == deskId && d.Location.LocationName == deskLocation);
                if (desk == null) return BadRequest("Desk does not exist.");
                desk.IsAvailable = IsAvailable;
                await _context.SaveChangesAsync();
                return Ok();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int deskId, string deskLocation, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                if (u == null ||
                    u.Password != user.Password ||
                    !u.IsAdmin) return BadRequest();
                var deleteDesk = _context.Desks.FirstOrDefault(d => d.DeskId == deskId && d.Location.LocationName == deskLocation);
                if (deleteDesk == null) return NotFound();
                if (_context.Reservations.Any(r => r.Desk.Equals(deleteDesk) &&
                                            (r.EndDate > DateTime.Now ||
                                            r.StartDate > DateTime.Now))) return BadRequest("Cannot delete desk that is reserved!");
                _context.Remove(deleteDesk);
                await _context.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
    }
}
