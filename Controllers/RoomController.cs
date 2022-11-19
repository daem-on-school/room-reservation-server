using Microsoft.AspNetCore.Mvc;
using RoomReservation.Model;

namespace RoomReservation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoomController : ControllerBase
    {

        private readonly ILogger<RoomController> _logger;
        private AppDbContext _db;

        public RoomController(ILogger<RoomController> logger, AppDbContext appDbContext)
        {
            _logger = logger;
            _db = appDbContext;
        }

        [HttpGet(Name = "GetAllRooms")]
        public IEnumerable<Room> GetAll()
        {
            return _db.Rooms;
        }

        [HttpGet("{name}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Room> Get(string name)
        {
            var result = _db.Rooms.Find(name);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult Post([FromBody] Room value)
        {
            if (_db.Rooms.Find(value.Name) != null) return Conflict();
            if (value.Keywords.Any(k => k.Contains(","))) return BadRequest();
            _db.Rooms.Add(value);
            _db.SaveChanges();
            return CreatedAtAction(nameof(Get), new { name = value.Name }, value);
        }

        [HttpDelete("{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult Delete(string name)
        {
            var room = _db.Rooms.Find(name);
            if (room == null) return NoContent();
            _db.Rooms.Remove(room);
            _db.SaveChanges();
            return NoContent();
        }
    }
}