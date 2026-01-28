using EquipOps.Model.Vendor;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
    public class VendorRequestValidator : AbstractValidator<VendorRequest>
    {
        public VendorRequestValidator()
        {
            RuleFor(x => x.name)
               .NotEmpty().WithMessage("vendor name is required.")
               .MaximumLength(200).WithMessage("vendor name cannot exceed 200 characters.");

            RuleFor(x => x.service_type)
               .NotEmpty().WithMessage("contact name is required.")
               .MaximumLength(200).WithMessage("contact name cannot exceed 200 characters.");

            RuleFor(x => x.email)
                .Matches(@"^[^\s@]+@[^\s@]+\.com$")
                .WithMessage("Email must be a valid .com email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.email));

            RuleFor(x => x.phone)
                .Matches(@"^[0-9]{10}$")
                .WithMessage("Phone number must be exactly 10 digits.")
                .When(x => !string.IsNullOrWhiteSpace(x.phone));
        }
    }
}
