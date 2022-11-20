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
}