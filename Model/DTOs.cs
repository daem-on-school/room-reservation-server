using System.ComponentModel.DataAnnotations;

namespace RoomReservation.Model
{
	public record EventCreationDTO(
		[Required] string Title,
		[Required] string Description,
		DateTime Start,
		DateTime End,
		bool IsPublic
	);

	public record LoginDTO(
		[Required] string Username,
		[Required] string Password
	);

	public record RegisterDTO(
		[Required] string AdminToken,
		[Required] string Username,
		[Required] string Password,
		[Required] string Email,
		[Required] string Role
	);

	public record RoomDTO(
		[Required] string Name,
		[Required] string[] Keywords
	);

	public record TimeSpanDTO(
		[Required] DateTime Start,
		[Required] DateTime End
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
		DateTime Start,
		DateTime End,
		List<RoomDTO> Reservations
	);
}