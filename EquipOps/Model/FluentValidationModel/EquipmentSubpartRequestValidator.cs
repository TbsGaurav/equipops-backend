using EquipOps.Model.EquipmentSubpart;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
    public class EquipmentSubpartRequestValidator : AbstractValidator<EquipmentSubpartRequest>
    {
        public EquipmentSubpartRequestValidator() 
        {
            RuleFor(x => x.equipment_id)
            .GreaterThan(0)
            .WithMessage("Equipment is required.");

            RuleFor(x => x.subpart_name)
                .NotEmpty()
                .WithMessage("Subpart name is required.")
                .MaximumLength(200)
                .WithMessage("Subpart name cannot exceed 200 characters.");

            RuleFor(x => x.description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.description))
                .WithMessage("Description cannot exceed 500 characters.");
        }
    }
}
