namespace RoomReservation.Model
{
    public class Room
    {
        public string Name { get; set; }

        public string[] Keywords { get; set; }

        public List<Event> Reservations { get; set; }

		public Room(string name, string[] keywords, List<Event> reservations)
		{
			Name = name;
			Keywords = keywords;
			Reservations = reservations;
		}

		public Room()
		{
            Name = "";
            Keywords = new string[0];
            Reservations = new List<Event>();
		}

		public static implicit operator RoomDTO(Room v)
			=> new RoomDTO(v.Name, v.Keywords);
	}
}
