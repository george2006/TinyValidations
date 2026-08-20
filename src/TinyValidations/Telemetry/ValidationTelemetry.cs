using System.Diagnostics;

namespace TinyValidations
{
    internal static class ValidationTelemetry
    {
        private const string ValidationOutcomeAttribute = "tiny.validation.outcome";
        private const string ValidationFailureCountAttribute = "tiny.validation.failure.count";
        private const string SuccessOutcome = "success";
        private const string FailureOutcome = "failure";

        public static void RecordResult(ValidationResult result)
        {
            var activity = Activity.Current;
            if (activity is null)
            {
                return;
            }

            activity.SetTag(
                ValidationOutcomeAttribute,
                result.IsValid ? SuccessOutcome : FailureOutcome);
            activity.SetTag(ValidationFailureCountAttribute, result.Errors.Count);
        }
    }
}
