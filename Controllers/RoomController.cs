using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomReservation.Model;
using RoomReservation.Services;

namespace RoomReservation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {

        private readonly ILogger<RoomController> _logger;
        private AppDbContext _db;
        private readonly ReservationService _reservation;

        public RoomController(ILogger<RoomController> logger, AppDbContext appDbContext, ReservationService reservation)
        {
            _logger = logger;
            _db = appDbContext;
            _reservation = reservation;
        }

        [HttpGet(Name = "GetAllRooms")]
        public IEnumerable<RoomDTO> GetAll()
        {
            return _db.Rooms.Select(r => (RoomDTO)r);
        }

        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<RoomDTO> Get(string name)
        {
            var result = _db.Rooms.Find(name);
            return result == null ? NotFound() : Ok(
                new RoomDTO(Name: result.Name, Keywords: result.Keywords)
            );
        }

        [HttpGet("{name}/events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<EventSummaryDTO>> GetEvents(string name)
        {
            var room = _db.Rooms.Find(name);
            if (room == null) return NotFound();
            var showPrivate = User.Identity?.IsAuthenticated ?? false;
            
            _db.Entry(room).Collection(r => r.Reservations).Load();
            IEnumerable<Event> results = room.Reservations;
            if (!showPrivate) results = results.Where(e => e.IsPublic);
            return Ok(results.Select(e => (EventSummaryDTO)e));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] RoomDTO value)
        {
            if (_db.Rooms.Find(value.Name) != null) return Conflict();
            if (value.Keywords.Any(k => k.Contains(','))) return BadRequest();
            _db.Rooms.Add(new Room() {
                Name = value.Name,
                Keywords = value.Keywords
            });
            _db.SaveChanges();
            return CreatedAtAction(nameof(Get), new { name = value.Name }, value);
        }

        [HttpDelete("{name}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult Delete(string name)
        {
            var room = _db.Rooms.Find(name);
            if (room == null) return NoContent();
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpGet("find-time/{name}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<TimeSpanDTO>> FindAvailableTimes(string name, [FromQuery] TimeSpanDTO timeSpan)
        {
            var room = _db.Rooms.Find(name);
            if (room == null) return NotFound();
			return Ok(_reservation.FindAvailableTimes(room, timeSpan));
        }
    }
}