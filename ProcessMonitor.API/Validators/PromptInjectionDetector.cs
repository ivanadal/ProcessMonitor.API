namespace ProcessMonitor.API.Validators
{
    public static class PromptInjectionDetector
    {
        private static readonly string[] Indicators =
        {
        "ignore previous",
        "forget instructions",
        "system:",
        "assistant:",
        "user:",
        "### instruction",
        "new instructions:",
        "follow my instructions"
    };

        public static bool HasPromptInjection(string input)
        {
            return Indicators.Any(ind =>
                input.Contains(ind, StringComparison.OrdinalIgnoreCase));
        }
    }
}
