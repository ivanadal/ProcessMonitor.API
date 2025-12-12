using FluentValidation;
using ProcessMonitor.API.DTOs;

namespace ProcessMonitor.API.Validators
{
    public class AnalyzeRequestValidator : AbstractValidator<AnalyzeRequest>
    {
        public AnalyzeRequestValidator()
        {
            RuleFor(x => x.Action)
                .Must(NotContainPromptInjection)
                .WithMessage("Action contains invalid or unsafe content.")
                .NotEmpty()
                .WithMessage("Action cannot be empty.");

            RuleFor(x => x.Guideline)
                .Must(NotContainPromptInjection)
                .WithMessage("Guideline contains invalid or unsafe content.")
                .NotEmpty()
                .WithMessage("Guideline cannot be empty.");
        }    

     private bool NotContainPromptInjection(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return true;

            return !PromptInjectionDetector.HasPromptInjection(input);
        }
    }
}
