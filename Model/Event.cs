namespace RoomReservation.Model {
	public class Event {
		public int Id { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public bool IsPublic { get; set; }
		public User Organizer { get; set; }
		public List<Room> Reservations { get; set; }

		public Event(int id, string title, string description, DateTime start, DateTime end, bool isPublic, User organizer, List<Room> reservations)
		{
			Id = id;
			Title = title;
			Description = description;
			Start = start;
			End = end;
			IsPublic = isPublic;
			Organizer = organizer;
			Reservations = reservations;
		}

		public Event()
		{
		}
	}
}