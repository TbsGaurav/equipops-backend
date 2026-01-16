using EquipOps.Model.Dashboard;
using FluentValidation;

namespace EquipOps.Model.FluentValidationModel
{
	public class DashboardCategoryValidator : AbstractValidator<DashboardCategoryRequest>
	{
		public DashboardCategoryValidator()
		{
			RuleFor(x => x.OrganizationId)
				.GreaterThan(0).WithMessage("Organization is required.");

			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required.")
				.MaximumLength(100).WithMessage("Name max length is 100.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description max length is 500.");
		}
	}
}
