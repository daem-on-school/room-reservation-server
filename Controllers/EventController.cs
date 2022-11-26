using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomReservation.Model;
using RoomReservation.Services;

namespace RoomReservation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {

        private readonly ILogger<EventController> _logger;
        private AppDbContext _db;
		private readonly UserManager<IdentityUser> _userManager;
        private readonly ReservationService _reservation;

        public EventController(
			ILogger<EventController> logger,
			AppDbContext appDbContext,
			UserManager<IdentityUser> userManager,
            ReservationService reservation
        ) {
            _logger = logger;
            _db = appDbContext;
            _userManager = userManager;
            _reservation = reservation;
        }

        private bool hasPermission(Event e) {
            if (!User.Identity?.IsAuthenticated ?? false) return false;
            var user = _userManager.GetUserAsync(User).Result;
            return e.Organizer == user || User.IsInRole("Admin");
        }

        [HttpGet(Name = "GetAll")]
        public IEnumerable<EventSummaryDTO> GetAll([FromQuery] bool future = false)
        {
            var showPrivate = User.Identity?.IsAuthenticated ?? false;
            IEnumerable<Event> results = _db.Events;
            if (!showPrivate) results = results.Where(e => e.IsPublic);
            if (future) results = results.Where(e => e.Start > DateTime.Now);
            return results.Select(e => (EventSummaryDTO)e);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<EventWithReservationDTO> Get(int id)
        {
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            if (!result.IsPublic
                && (!User.Identity?.IsAuthenticated ?? false))
                return Unauthorized();
            _db.Entry(result).Collection(e => e.Reservations).Load();
            _db.Entry(result).Reference(e => e.Organizer).Load();
            return Ok(new EventWithReservationDTO(
                Id: result.Id,
                Title: result.Title,
                Description: result.Description,
                OrganizerName: result.Organizer.UserName,
                Start: result.Start,
                End: result.End,
                Reservations: result.Reservations
                    .Select(r => new RoomDTO(r.Name, r.Keywords))
                    .ToList(),
                IsPublic: result.IsPublic
            ));
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Post([FromBody] EventCreationDTO value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(value.Start >= value.End)
            {
                ModelState.AddModelError("Start", "Start time must be before end time");
                return BadRequest(ModelState);
            }

            var created = new Event
            {
                Title = value.Title,
                Description = value.Description,
                Start = value.Start,
                End = value.End,
                IsPublic = value.IsPublic,
                Organizer = await _userManager.GetUserAsync(User),
                Reservations = new List<Room>()
            };

            _db.Events.Add(created);
            _db.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        private void AddConflictsToModelState(IEnumerable<Conflict> conflicts)
        {
            ModelState.AddModelError("Rooms", "One or more rooms are already reserved");
            foreach (var c in conflicts)
            {
                _db.Entry(c.Event).Reference(e => e.Organizer).Load();
                ModelState.AddModelError("Rooms", $"{c.Room.Name} is reserved, contact {c.Event.Organizer.Email}");
            }
        }

        [HttpPost("{id}/reservations")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult PostReserve(int id, [FromBody] List<string> rooms)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = _db.Events.Find(id);
            if (result == null) return NotFound();

            var roomList = rooms.Select(r => _db.Rooms.Find(r)).OfType<Room>().ToList();
            if (roomList.Count != rooms.Count)
            {
                ModelState.AddModelError("Rooms", "One or more rooms not found");
                return BadRequest(ModelState);
            }

            var conflicts = _reservation.FindConflicts(roomList, result);
            if (conflicts.Any())
            {
                AddConflictsToModelState(conflicts);
                return BadRequest(ModelState);
            }

			_db.Entry(result).Collection(r => r.Reservations).Load();
            if (!_reservation.ReserveRooms(result, roomList.ToArray()))
                return BadRequest("Could not reserve rooms");

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult Put(int id, [FromBody] EventCreationDTO value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            if (!hasPermission(result)) return Forbid();

            if (value.Start >= value.End)
            {
                ModelState.AddModelError("Start", "Start time must be before end time");
                return BadRequest(ModelState);
            }

            _db.Entry(result).Collection(e => e.Reservations).Load();
            var conflicts = _reservation.FindConflicts(
                result.Reservations, new TimeSpanDTO(value.Start, value.End)
            );
            conflicts = conflicts.Where(c => c.Event.Id != id);
            if (conflicts.Any())
            {
                AddConflictsToModelState(conflicts);
                return BadRequest(ModelState);
            }

            result.Title = value.Title;
            result.Description = value.Description;
            result.Start = value.Start;
            result.End = value.End;
            result.IsPublic = value.IsPublic;
            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            if (!hasPermission(result)) return Forbid();
            _db.Events.Remove(result);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}/reservations")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeleteReservations(int id, [FromBody] List<string> rooms)
        {
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            if (!hasPermission(result)) return Forbid();

            var roomList = rooms.Select(r => _db.Rooms.Find(r)).OfType<Room>().ToList();
            if (roomList.Count != rooms.Count)
            {
                ModelState.AddModelError("Rooms", "One or more rooms not found");
                return BadRequest(ModelState);
            }

			_db.Entry(result).Collection(r => r.Reservations).Load();
            _reservation.CancelReservations(result, roomList.ToArray());

            return NoContent();
        }        

        [HttpGet("find-room/{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<RoomDTO>> FindAvailableRooms(int id)
        {
            var ev = _db.Events.Find(id);
            if (ev == null) return NotFound();
			return Ok(_reservation.FindAvailableRooms(ev).Select(r => (RoomDTO)r));
        }
    }
}