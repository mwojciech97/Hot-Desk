using HotDeskAPI.DataContext;
using HotDeskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotDeskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly HotDeskDBContext _context;
        public ReservationController(HotDeskDBContext context) => _context = context;

        [HttpPost("{id}")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetById(int id, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                var reservation = await _context.Reservations.Include(d => d.Desk).Include(u => u.User).Include(l => l.Desk.Location).FirstOrDefaultAsync(r => r.Id == id);
                if (reservation == null) return NotFound();
                if (u == null ||
                   (u.Username != reservation.User.Username ||
                   u.Password != user.Password) &&
                   !u.IsAdmin) return BadRequest("You do not have permission to see requested reservation!");
                return reservation == null ? NotFound() : Ok(reservation);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPost("getreservations")]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Reservation), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetReservations(User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                var r = await _context.Reservations.Include(d => d.Desk)
                                                    .Include(u => u.User)
                                                    .Include(l => l.Desk.Location)
                                                    .ToListAsync();
                if (u == null ||
                    u.Username != user.Password ||
                    !u.IsAdmin) return BadRequest("Wrong username or password.");
                return Ok(r);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPost("createreservation")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReservation(Reservation reservation)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == reservation.User.Username);
                var reserved = await _context.Reservations.Include(u => u.User).ToListAsync();
                if (u == null || 
                    u.Password != reservation.User.Password) return BadRequest("Wrong username or password.");

                var l = await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == reservation.Desk.Location.LocationName);
                var d = await _context.Desks.FirstOrDefaultAsync(d => d.DeskId == reservation.Desk.DeskId && d.Location == l);
                if (d == null ||
                    _context.Reservations.Any(r => r.Desk.Equals(d) && ((
                        r.StartDate.Date <= reservation.EndDate.Date &&
                        r.EndDate.Date >= reservation.StartDate.Date) ||
                        reservation.StartDate.Date <= r.EndDate.Date &&
                        reservation.EndDate.Date >= r.StartDate.Date)) ||
                    !d.IsAvailable) return BadRequest("Desk does not exists or is unavailable.");
                reservation.Desk = d;
                reservation.User = u;
                TimeSpan check = (reservation.EndDate - reservation.StartDate).Add(TimeSpan.FromDays(1));
                TimeSpan checkAll = TimeSpan.Zero;
                reserved.ForEach(c =>
                {
                    if (c.User.Equals(u))
                    {
                        if (c.EndDate - c.StartDate == TimeSpan.FromDays(0)) checkAll += TimeSpan.FromDays(1);
                        else checkAll += (c.EndDate - c.StartDate).Add(TimeSpan.FromDays(1));
                    }
                });
                if(check.TotalDays > 5 || 
                    checkAll.Days + check.TotalDays > 5) return BadRequest("You cannot have reservations for more than 5 days.");
                if (reservation.StartDate.Date < DateTime.Now.Date ||
                   reservation.StartDate.Date > reservation.EndDate.Date) return BadRequest("Invalid data provided.");

              
                await _context.Reservations.AddAsync(reservation);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = reservation.Id}, reservation);
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
        [HttpPut("updatereservation/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, string deskLocation, int deskId, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                var r = await _context.Reservations.FindAsync(id);

                if (u == null ||
                    r == null ||
                    u != r.User ||
                    user.Password != u.Password) return BadRequest("Reservation does not exists or you do not have permission to update it.");

                if (r.StartDate.Date.Equals(DateTime.Now.Date.AddDays(-1)) ||
                    r.EndDate.Date <= DateTime.Now.Date)
                    return BadRequest("You cannot change this reservation.");
                var d = await _context.Desks.FirstOrDefaultAsync(d => d.DeskId == deskId && d.Location.LocationName == deskLocation);
                if (d == null) return BadRequest("Desk does not exist or is unavailable.");
                r.Desk = d;
                _context.Entry(d).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }
        [HttpDelete("deletereservation/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id, User user)
        {
            try
            {
                var u = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
                var deleteReservation = await _context.Reservations.Include(u => u.User).FirstOrDefaultAsync(r => r.Id == id);
                if (deleteReservation == null) return NotFound();
                if (u == null ||
                    deleteReservation.User.Username != u.Username ||
                    deleteReservation.User.Password != user.Password) return BadRequest();
                _context.Remove(deleteReservation);
                await _context.SaveChangesAsync();
                return NoContent();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

    }
}
