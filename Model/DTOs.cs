using System.ComponentModel.DataAnnotations;

namespace RoomReservation.Model
{
	public record EventCreationDTO(
		string Title,
		string Description,
		DateTime Start,
		DateTime End,
		bool IsPublic
	);

	public record LoginDTO(
		string Username,
		string Password
	);

	public record RegisterDTO(
		string AdminToken,
		string Username,
		string Password,
		string Email,
		string Role
	);

	public record RoomDTO(
		string Name,
		string[] Keywords
	);

	public record TimeSpanDTO(
		DateTime Start,
		DateTime End
	);

	public record EventSummaryDTO(
		int Id,
		string Title,
		string Description,
		DateTime Start,
		DateTime End
	);

	public record EventWithReservationDTO(
		int Id,
		string Title,
		string Description,
		string OrganizerName,
		DateTime Start,
		DateTime End,
		List<RoomDTO> Reservations,
		bool IsPublic
	);
}