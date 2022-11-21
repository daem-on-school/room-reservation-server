using RoomReservation.Model;

namespace RoomReservation.Services
{
	public class ReservationService {
		private readonly AppDbContext _db;

		public ReservationService(AppDbContext db) {
			_db = db;
		}

		private bool Overlaps(Event e, TimeSpanDTO ts) {
			return e.Start < ts.End && ts.Start < e.End;
		}

		private bool IsRoomAvailable(Room room, TimeSpanDTO ts) {
			return room.Reservations.All(e => !Overlaps(e, ts));
		}

		public bool ReserveRooms(Event ev, Room[] rooms) {
			if (rooms.Any(r => !IsRoomAvailable(r, ev))) {
				return false;
			}

			foreach (var room in rooms) {
				room.Reservations.Add(ev);
			}

			_db.SaveChanges();
			return true;
		}

		public IEnumerable<Event> Conflicts(Room room, TimeSpanDTO ts) {
			return room.Reservations.Where(e => Overlaps(e, ts));
		}

		public Dictionary<Room, IEnumerable<Event>> Conflicts(IEnumerable<Room> rooms, TimeSpanDTO ts) {
			return rooms.ToDictionary(r => r, r => Conflicts(r, ts));
		}

		public void CancelReservations(Event ev, Room[] rooms)
		{
			foreach (var room in rooms)
			{
				room.Reservations.Remove(ev);
			}

			_db.SaveChanges();
		}

		public void CancelAllReservations(Event ev)
		{
			foreach (var room in ev.Reservations) room.Reservations.Remove(ev);
			ev.Reservations.Clear();
			_db.SaveChanges();
		}

		public void CancelAllReservations(Room room)
		{
			foreach (var ev in room.Reservations) ev.Reservations.Remove(room);
			room.Reservations.Clear();
			_db.SaveChanges();
		}

		public IEnumerable<Room> FindAvailableRooms(Event ev) {
			return _db.Rooms.Where(r => IsRoomAvailable(r, ev));
		}

		public IEnumerable<TimeSpanDTO> FindAvailableTimes(Room room, TimeSpanDTO limit) {
			var reservations = room.Reservations
				.Where(e => e.Start < limit.End && limit.Start < e.End)
				.OrderBy(e => e.Start);
			var start = limit.Start;
			foreach (var reservation in reservations) {
				if (reservation.Start > start) {
					yield return new TimeSpanDTO(start, reservation.Start);
				}

				start = reservation.End;
			}

			if (start < limit.End) {
				yield return new TimeSpanDTO(start, limit.End);
			}
		}
	}
}