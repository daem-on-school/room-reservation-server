using Microsoft.AspNetCore.Mvc;
using RoomReservation.Model;

namespace RoomReservation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {

        private readonly ILogger<EventController> _logger;
        private AppDbContext _db;

        public EventController(ILogger<EventController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _db = appDbContext;
            logger.LogInformation("EventController created");
        }

        [HttpGet(Name = "GetAllEvents")]
        public IEnumerable<Event> GetAll()
        {
            return _db.Events;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Event> Get(int id)
        {
            var result = _db.Events.Find(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] EventCreationDTO value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = new Event
            {
                Title = value.Title,
                Description = value.Description,
                Start = value.Start,
                End = value.End,
                IsPublic = value.IsPublic,
                Reservations = new List<Room>()
            };

            _db.Events.Add(created);
            _db.SaveChanges();

            return CreatedAtRoute("GetAllEvents", new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Put(int id, [FromBody] EventCreationDTO value)
        {
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            result.Title = value.Title;
            result.Description = value.Description;
            result.Start = value.Start;
            result.End = value.End;
            result.IsPublic = value.IsPublic;
            _db.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult Delete(int id)
        {
            var result = _db.Events.Find(id);
            if (result == null) return NotFound();
            _db.Events.Remove(result);
            _db.SaveChanges();
            return NoContent();
        }
    }
}