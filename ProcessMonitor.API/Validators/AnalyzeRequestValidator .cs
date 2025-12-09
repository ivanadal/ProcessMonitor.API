using FluentValidation;
using ProcessMonitor.API.Models;

namespace ProcessMonitor.API.Validators
{
    public class AnalyzeRequestValidator : AbstractValidator<AnalyzeRequest>
    {
        public AnalyzeRequestValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty();

            RuleFor(x => x.Guideline)
                .NotEmpty();
        }
    }
}
