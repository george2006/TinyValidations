using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TinyValidations.Tests;

public sealed class RuntimeTests
{
    [Fact]
    public void Validation_error_stores_member_and_message()
    {
        var error = new ValidationError("Email", "Email is required.");

        Assert.Equal("Email", error.Member);
        Assert.Equal("Email is required.", error.Message);
    }

    [Theory]
    [InlineData(null, "Email is required.")]
    [InlineData("", "Email is required.")]
    [InlineData("   ", "Email is required.")]
    [InlineData("Email", null)]
    [InlineData("Email", "")]
    [InlineData("Email", "   ")]
    public void Validation_error_rejects_missing_member_or_message(string? member, string? message)
    {
        Assert.Throws<ArgumentException>(() => new ValidationError(member!, message!));
    }

    [Fact]
    public void Empty_error_collection_returns_valid_result()
    {
        var errors = new ValidationErrorCollection();

        var result = errors.ToResult();

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Error_collection_returns_invalid_result_when_errors_exist()
    {
        var errors = new ValidationErrorCollection();

        errors.Add("Email", "Email is required.");

        var result = errors.ToResult();

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Error_collection_rejects_null_ranges()
    {
        var errors = new ValidationErrorCollection();

        Assert.Throws<ArgumentNullException>(() => errors.AddRange(null!));
    }

    [Fact]
    public void Invalid_result_uses_error_snapshot()
    {
        var errors = new List<ValidationError>
        {
            new ValidationError("Email", "Email is required.")
        };

        var result = new ValidationResult(errors);
        errors.Add(new ValidationError("Name", "Name is required."));

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void Validation_result_rejects_null_error_collection()
    {
        Assert.Throws<ArgumentNullException>(() => new ValidationResult(null!));
    }

    [Fact]
    public async Task Validator_returns_valid_result_when_no_runner_is_registered()
    {
        var validator = BuildValidator();

        var result = await validator.ValidateAsync(new CommandWithoutValidation());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validator_rejects_null_instances()
    {
        var validator = BuildValidator();

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await validator.ValidateAsync<CommandWithoutValidation>(null!));
    }

    [Fact]
    public void Use_tiny_validations_registers_validator_once()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();
        services.UseTinyValidations();

        var count = CountValidatorRegistrations(services);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Use_tiny_validations_rejects_null_services()
    {
        Assert.Throws<ArgumentNullException>(() => TinyValidationServiceCollectionExtensions.UseTinyValidations(null!));
    }

    [Fact]
    public void Bootstrap_apply_rejects_null_services()
    {
        Assert.Throws<ArgumentNullException>(() => TinyValidationBootstrap.Apply(null!));
    }

    [Fact]
    public void Use_tiny_validations_can_be_called_twice_without_duplicate_runner_registrations()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();
        services.UseTinyValidations();

        var count = CountRegistrations<ITinyValidationRunner<CreateProfile>>(services);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Bootstrap_can_apply_contributions_to_multiple_service_collections()
    {
        var first = new ServiceCollection();
        var second = new ServiceCollection();

        first.UseTinyValidations();
        second.UseTinyValidations();

        var firstCount = CountRegistrations<ITinyValidationRunner<CreateProfile>>(first);
        var secondCount = CountRegistrations<ITinyValidationRunner<CreateProfile>>(second);

        Assert.Equal(1, firstCount);
        Assert.Equal(1, secondCount);
    }

    [Fact]
    public void Duplicate_contribution_does_not_register_duplicate_services()
    {
        var services = new ServiceCollection();

        TinyValidationBootstrap.AddContribution(new DuplicateTestContribution());
        TinyValidationBootstrap.AddContribution(new DuplicateTestContribution());
        services.UseTinyValidations();

        var count = CountRegistrations<DuplicateContributionMarker>(services);

        Assert.Equal(1, count);
    }

    [Fact]
    public void Bootstrap_exposes_generated_structure_without_running_validation()
    {
        var snapshot = TinyValidationBootstrap.GetValidations();

        var validation = Assert.Single(
            snapshot,
            candidate => candidate.ValidatedTypeIdentity == typeof(CreateProfile).FullName);
        Assert.Equal(typeof(CreateProfileValidation).FullName, validation.DeclarationIdentity);
        Assert.Contains(validation.Rules, rule => rule.MemberPath == "Age" && rule.Kind == "AtLeast");

        var customValidation = Assert.Single(
            snapshot,
            candidate => candidate.ValidatedTypeIdentity == typeof(CreateTeam).FullName);
        Assert.Contains(typeof(ReservedTeamNameRule).FullName, customValidation.CustomRuleIdentities);
    }

    [Fact]
    public void Validation_structure_copies_rule_and_custom_rule_inputs()
    {
        var rules = new[] { new TinyValidationRuleStructure("Name", "Required") };
        var customRules = new[] { typeof(ReservedTeamNameRule) };
        var structure = new TinyValidationStructure(
            typeof(CreateTeam),
            typeof(CreateTeamValidation),
            rules,
            customRules);

        rules[0] = new TinyValidationRuleStructure("Changed", "Email");
        customRules[0] = typeof(RuntimeTests);

        var rule = Assert.Single(structure.Rules);
        Assert.Equal("Name", rule.MemberPath);
        Assert.Equal(typeof(ReservedTeamNameRule), Assert.Single(structure.CustomRuleTypes));
        Assert.Equal(typeof(ReservedTeamNameRule).FullName, Assert.Single(structure.CustomRuleIdentities));
    }

    [Fact]
    public void Validation_structure_exposes_types_and_compatible_identities()
    {
        var structure = new TinyValidationStructure(
            typeof(CreateTeam),
            typeof(CreateTeamValidation),
            Array.Empty<TinyValidationRuleStructure>(),
            new[] { typeof(ReservedTeamNameRule) });

        Assert.Equal(typeof(CreateTeam), structure.ValidatedType);
        Assert.Equal(typeof(CreateTeamValidation), structure.DeclarationType);
        Assert.Equal(typeof(ReservedTeamNameRule), Assert.Single(structure.CustomRuleTypes));
        Assert.Equal(typeof(CreateTeam).FullName, structure.ValidatedTypeIdentity);
        Assert.Equal(typeof(CreateTeamValidation).FullName, structure.DeclarationIdentity);
        Assert.Equal(
            typeof(ReservedTeamNameRule).FullName,
            Assert.Single(structure.CustomRuleIdentities));
    }

    [Fact]
    public void Validation_structure_rules_cannot_be_modified()
    {
        var original = new TinyValidationRuleStructure("Name", "Required");
        var structure = new TinyValidationStructure(
            typeof(CreateTeam),
            typeof(CreateTeamValidation),
            new[] { original },
            Array.Empty<Type>());
        var rules = Assert.IsAssignableFrom<IList<TinyValidationRuleStructure>>(structure.Rules);

        Assert.Throws<NotSupportedException>(
            () => rules[0] = new TinyValidationRuleStructure("Changed", "Email"));
        Assert.Same(original, Assert.Single(structure.Rules));
    }

    [Fact]
    public void Validation_structure_custom_rule_identities_cannot_be_modified()
    {
        var structure = new TinyValidationStructure(
            typeof(CreateTeam),
            typeof(CreateTeamValidation),
            Array.Empty<TinyValidationRuleStructure>(),
            new[] { typeof(ReservedTeamNameRule) });
        var customRules = Assert.IsAssignableFrom<IList<string>>(structure.CustomRuleIdentities);

        Assert.Throws<NotSupportedException>(
            () => customRules[0] = typeof(RuntimeTests).FullName!);
        Assert.Equal(
            typeof(ReservedTeamNameRule).FullName,
            Assert.Single(structure.CustomRuleIdentities));
    }

    [Fact]
    public void Validation_structure_custom_rule_types_cannot_be_modified()
    {
        var structure = new TinyValidationStructure(
            typeof(CreateTeam),
            typeof(CreateTeamValidation),
            Array.Empty<TinyValidationRuleStructure>(),
            new[] { typeof(ReservedTeamNameRule) });
        var customRules = Assert.IsAssignableFrom<IList<Type>>(structure.CustomRuleTypes);

        Assert.Throws<NotSupportedException>(
            () => customRules[0] = typeof(RuntimeTests));
        Assert.Equal(
            typeof(ReservedTeamNameRule),
            Assert.Single(structure.CustomRuleTypes));
    }

    [Fact]
    public void Bootstrap_structure_snapshot_is_deterministically_ordered()
    {
        var snapshot = TinyValidationBootstrap.GetValidations();
        var expected = snapshot
            .OrderBy(validation => validation.ValidatedTypeIdentity, StringComparer.Ordinal)
            .ThenBy(validation => validation.DeclarationIdentity, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, snapshot);
    }

    [Fact]
    public void Bootstrap_returns_a_new_structure_snapshot_for_each_read()
    {
        var first = TinyValidationBootstrap.GetValidations();
        var second = TinyValidationBootstrap.GetValidations();

        Assert.NotSame(first, second);
        Assert.Equal(first, second);
    }

    [Fact]
    public void Generated_validation_structure_is_materialized_once()
    {
        var firstSnapshot = TinyValidationBootstrap.GetValidations();
        var secondSnapshot = TinyValidationBootstrap.GetValidations();

        var first = Assert.Single(
            firstSnapshot,
            candidate => candidate.ValidatedTypeIdentity == typeof(CreateProfile).FullName);
        var second = Assert.Single(
            secondSnapshot,
            candidate => candidate.ValidatedTypeIdentity == typeof(CreateProfile).FullName);

        Assert.Same(first, second);
    }

    [Fact]
    public void Validation_structure_is_collected_lazily_and_only_once()
    {
        var contribution = new LazyStructureTestContribution();

        TinyValidationBootstrap.AddContribution(contribution);

        var readsBeforeCollection = contribution.StructureReads;

        TinyValidationBootstrap.GetValidations();
        TinyValidationBootstrap.GetValidations();

        Assert.Equal(0, readsBeforeCollection);
        Assert.Equal(1, contribution.StructureReads);
    }

    private static ITinyValidator BuildValidator()
    {
        var services = new ServiceCollection();

        services.UseTinyValidations();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<ITinyValidator>();
    }

    private static int CountValidatorRegistrations(IServiceCollection services)
    {
        return CountRegistrations<ITinyValidator>(services);
    }

    private static int CountRegistrations<TService>(IServiceCollection services)
    {
        return CountRegistrations(services, typeof(TService));
    }

    private static int CountRegistrations(IServiceCollection services, Type serviceType)
    {
        var count = 0;

        foreach (var service in services)
        {
            if (service.ServiceType != serviceType)
            {
                continue;
            }

            count++;
        }

        return count;
    }
}

public sealed record CommandWithoutValidation;

public sealed class DuplicateContributionMarker
{
}

public sealed class DuplicateTestContribution : ITinyValidationContribution
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<DuplicateContributionMarker>();
    }
}

public sealed class LazyStructureTestContribution :
    ITinyValidationContribution,
    ITinyValidationStructureContribution
{
    public int StructureReads { get; private set; }

    public IReadOnlyList<TinyValidationStructure> Validations
    {
        get
        {
            StructureReads++;

            return new[]
            {
                new TinyValidationStructure(
                    typeof(CommandWithoutValidation),
                    typeof(RuntimeTests),
                    Array.Empty<TinyValidationRuleStructure>(),
                    Array.Empty<Type>())
            };
        }
    }

    public void Register(IServiceCollection services)
    {
    }
}
