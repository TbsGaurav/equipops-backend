using EquipOps.Model.EquipmentCategory;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
    public class EquipmentCategoryRequestValidator: AbstractValidator<EquipmentCategoryRequest>
    {
        public EquipmentCategoryRequestValidator()
        {
            RuleFor(x => x.category_name)
               .NotEmpty().WithMessage("category name is required.")
               .MaximumLength(200).WithMessage("category name cannot exceed 200 characters.");

            RuleFor(x => x.description)
                .MaximumLength(500).WithMessage("description cannot exceed 500 characters.");
        }
    }
}
