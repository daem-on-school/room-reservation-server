namespace RoomReservation.Model {
	public enum Authority {
		Administrator,
		Organizer,
	}

	public class User {
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public Authority Authority { get; set; }

		public User(int id, string name, string email, string password, Authority authority)
		{
			Id = id;
			Name = name;
			Email = email;
			Password = password;
			Authority = authority;
		}
	}
}