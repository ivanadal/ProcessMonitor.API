using FluentValidation;
using ProcessMonitor.API.DTOs;
using ProcessMonitor.API.Models;

namespace ProcessMonitor.API.Validators
{
    public class HistoryQueryValidator : AbstractValidator<HistoryQuery>
    {
        public HistoryQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0.");
        }
    }
}
