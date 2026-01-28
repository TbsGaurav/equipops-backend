using EquipOps.Model.SlaMetrics;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
	public sealed class SlaMetricsRequestValidator : AbstractValidator<SlaMetricsRequest>
	{
		public SlaMetricsRequestValidator()
		{
			RuleFor(x => x.OrganizationId)
				.GreaterThan(0)
				.WithMessage("Organization is required.");

			RuleFor(x => x.EquipmentId)
				.GreaterThan(0)
				.WithMessage("Equipment is required.");

			RuleFor(x => x.SubpartId)
				.GreaterThan(0)
				.WithMessage("Subpart is required.");

			RuleFor(x => x.PeriodStart)
				.NotEmpty()
				.WithMessage("Period start date is required.");

			RuleFor(x => x.PeriodEnd)
				.NotEmpty()
				.WithMessage("Period end date is required.")
				.GreaterThanOrEqualTo(x => x.PeriodStart)
				.WithMessage("Period end date must be greater than or equal to start date.");

			RuleFor(x => x.DowntimeMinutes)
				.GreaterThanOrEqualTo(0)
				.WithMessage("Downtime minutes cannot be negative.");
		}
	}
}
