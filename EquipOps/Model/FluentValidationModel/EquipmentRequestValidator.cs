using EquipOps.Model.Equipment;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
	public class EquipmentRequestValidator : AbstractValidator<EquipmentRequest>
	{
		public EquipmentRequestValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Equipment name is required.")
				.MaximumLength(200).WithMessage("Equipment name cannot exceed 200 characters.");

			RuleFor(x => x.Status)
				.Must(s => s == 0 || s == 1)
				.WithMessage("Status must be 0 (Inactive) or 1 (Active).");

			RuleFor(x => x.PurchaseDate)
				.LessThanOrEqualTo(DateTime.Today)
				.When(x => x.PurchaseDate.HasValue)
				.WithMessage("Purchase date cannot be in the future.");

			// Optional string length safety
			RuleFor(x => x.Type).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Type));
			RuleFor(x => x.QrCode).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.QrCode));
			RuleFor(x => x.Location).MaximumLength(300).When(x => !string.IsNullOrEmpty(x.Location));
		}
	}
}
