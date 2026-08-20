using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests
{
    public sealed class ValidationTelemetryTests
    {
        [Fact]
        public async Task Successful_validation_enriches_the_current_activity()
        {
            var validator = BuildValidator(ValidationResult.Valid);
            using var activity = new Activity("application.operation").Start();

            var result = await validator.ValidateAsync(new TelemetryCommand());

            Assert.True(result.IsValid);
            Assert.Equal("success", activity.GetTagItem("tiny.validation.outcome"));
            Assert.Equal(0, activity.GetTagItem("tiny.validation.failure.count"));
        }

        [Fact]
        public async Task Failed_validation_records_only_the_outcome_and_failure_count()
        {
            var validationResult = new ValidationResult(
            [
                new ValidationError("Email", "Email is required."),
                new ValidationError("Name", "Name is required.")
            ]);
            var validator = BuildValidator(validationResult);
            using var activity = new Activity("application.operation").Start();

            var result = await validator.ValidateAsync(new TelemetryCommand());

            Assert.False(result.IsValid);
            Assert.Equal("failure", activity.GetTagItem("tiny.validation.outcome"));
            Assert.Equal(2, activity.GetTagItem("tiny.validation.failure.count"));
            Assert.Equal(
                ["tiny.validation.failure.count", "tiny.validation.outcome"],
                activity.TagObjects.Select(tag => tag.Key).OrderBy(key => key));
        }

        [Fact]
        public async Task Standalone_validation_does_not_create_an_activity()
        {
            var validator = BuildValidator(ValidationResult.Valid);
            var previousActivity = Activity.Current;
            Activity.Current = null;

            try
            {
                await validator.ValidateAsync(new TelemetryCommand());

                Assert.Null(Activity.Current);
            }
            finally
            {
                Activity.Current = previousActivity;
            }
        }

        [Fact]
        public async Task A_runner_exception_does_not_record_a_completed_validation_result()
        {
            var validator = BuildValidator(new ThrowingValidationRunner());
            using var activity = new Activity("application.operation").Start();

            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await validator.ValidateAsync(new TelemetryCommand()));

            Assert.Null(activity.GetTagItem("tiny.validation.outcome"));
            Assert.Null(activity.GetTagItem("tiny.validation.failure.count"));
        }

        private static ITinyValidator BuildValidator(ValidationResult result)
        {
            return BuildValidator(new StubValidationRunner(result));
        }

        private static ITinyValidator BuildValidator(ITinyValidationRunner<TelemetryCommand> runner)
        {
            var services = new ServiceCollection();
            services.AddSingleton(runner);
            services.UseTinyValidations();

            return services.BuildServiceProvider().GetRequiredService<ITinyValidator>();
        }

        private sealed record TelemetryCommand;

        private sealed class StubValidationRunner(ValidationResult result) :
            ITinyValidationRunner<TelemetryCommand>
        {
            public ValueTask<ValidationResult> ValidateAsync(
                TelemetryCommand instance,
                CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(result);
            }
        }

        private sealed class ThrowingValidationRunner : ITinyValidationRunner<TelemetryCommand>
        {
            public ValueTask<ValidationResult> ValidateAsync(
                TelemetryCommand instance,
                CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Validation runner failed.");
            }
        }
    }
}
