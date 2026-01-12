using System.Text.Json.Serialization;

namespace AuthService.Api.ViewModels.Response
{
	public class AuthResponseViewModel
	{
		public Guid? UserId { get; set; }
		public Guid? OrganizationId { get; set; }
		public string? OrganizationName { get; set; }
		public string? RoleName { get; set; }
		public string? WebsiteUrl { get; set; }
		public string? Email { get; set; }
		public string? PhotoUrl { get; set; }
		public string? FullName { get; set; }
		public string? NameInit { get; set; }

		[JsonIgnore]
		public string HashPassword { get; set; } = string.Empty;
		public string Token { get; set; } = string.Empty;
		public string[] menuPermissions { get; set; }
		public List<Subscription> subscription { get; set; } = new List<Subscription>();
	}
	public class Subscription
	{
		public Guid Id { get; set; }
		public Guid Organization_Id { get; set; }
		public Guid Subscription_Type_Id { get; set; }
		public string Type { get; set; } = string.Empty;
		public int Resume_Matching { get; set; }
		public int Interview_Create { get; set; }
		public int Interview_Schedule { get; set; }
		public decimal Price { get; set; }

	}
}
