using EquipOps.Model.EquipmentFailure;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
    public class EquipmentFailureRequestValidator :AbstractValidator<EquipmentFailureRequest>
    {
        public EquipmentFailureRequestValidator()
        {
            RuleFor(x => x.organization_id)
           .GreaterThan(0)
           .WithMessage("Organization is required.");

            RuleFor(x => x.equipment_id)
                .GreaterThan(0)
                .WithMessage("Equipment is required.");

            RuleFor(x => x.subpart_id)
                .GreaterThan(0)
                .When(x => x.subpart_id.HasValue)
                .WithMessage("Subpart must be a valid value.");

            RuleFor(x => x.failure_type)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.failure_type))
                .WithMessage("Failure type cannot exceed 100 characters.");

            RuleFor(x => x.description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.description))
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.downtime_minutes)
                .GreaterThanOrEqualTo(0)
                .When(x => x.downtime_minutes.HasValue)
                .WithMessage("Downtime minutes must be zero or greater.");
        }
    }
}
