namespace RoomReservation.Model
{
    public class Room
    {
        public string Name { get; set; }

        public string[] Keywords { get; set; }

        public Room(string name, string[] keywords)
        {
            Name = name;
            Keywords = keywords;
        }
    }
}
