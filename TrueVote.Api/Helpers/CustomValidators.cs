using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

#pragma warning disable IDE0046 // Convert to conditional expression
namespace TrueVote.Api.Helpers;

public abstract class NumberOfChoicesValidatorAttribute : ValidationAttribute
{
    protected readonly string PropertyName;
    protected readonly string RacePropertyName;
    protected string RacePropertyValue = string.Empty;
    public string GetRacePropertyValue()
    {
        return RacePropertyValue;
    }

    protected NumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName)
    {
        PropertyName = propertyName;
        RacePropertyName = racePropertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var propertyInfo = validationContext.ObjectType.GetProperty(PropertyName);
        if (propertyInfo is null)
        {
            return new ValidationResult($"Property not found.", [validationContext.MemberName]);
        }

        var candidatePropertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);
        if (candidatePropertyValue is not List<CandidateModel> and not List<BaseCandidateModel>)
        {
            return new ValidationResult($"Property '{PropertyName}' is not a valid List<CandidateModel> and not a valid List<BaseCandidateModel> type.", [validationContext.MemberName]);
        }

        var nameProperty = validationContext.ObjectType.GetProperty(RacePropertyName);
        if (nameProperty is not null)
        {
            RacePropertyValue = nameProperty.GetValue(validationContext.ObjectInstance)?.ToString() ?? string.Empty;
        }

        if (!validationContext.Items.TryGetValue("IsBallot", out var isBallotObj) || isBallotObj is not bool isBallot || !isBallot)
        {
            return ValidationResult.Success;
        }

        var selectedCount = ((IEnumerable) candidatePropertyValue).Cast<dynamic>().Count(c => c.Selected == true);

        return ValidateCount(value, selectedCount, validationContext);
    }

    protected abstract ValidationResult? ValidateCount(object? choicesValue, int count, ValidationContext validationContext);
}

public class MaxNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
{
    public MaxNumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName) : base(propertyName, racePropertyName) { }

    protected override ValidationResult? ValidateCount(object? choicesValue, int selectedCount, ValidationContext validationContext)
    {
        if (choicesValue is int maxNumberOfChoices && selectedCount > maxNumberOfChoices)
        {
            return new ValidationResult($"Number of selected items in '{PropertyName}' cannot exceed MaxNumberOfChoices for '{RacePropertyValue}'. MaxNumberOfChoices: {maxNumberOfChoices}, SelectedCount: {selectedCount}", [validationContext.MemberName]);
        }

        return ValidationResult.Success;
    }
}

public class MinNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
{
    public MinNumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName) : base(propertyName, racePropertyName) { }

    protected override ValidationResult? ValidateCount(object? choicesValue, int selectedCount, ValidationContext validationContext)
    {
        if (choicesValue is int minNumberOfChoices && selectedCount < minNumberOfChoices)
        {
            return new ValidationResult($"Number of selected items in '{PropertyName}' must be greater or equal to MinNumberOfChoices for '{RacePropertyValue}'. MinNumberOfChoices: {minNumberOfChoices}, Count: {selectedCount}", [validationContext.MemberName]);
        }

        return ValidationResult.Success;
    }
}

public class BallotIntegrityCheckerAttribute : ValidationAttribute
{
    private readonly string _electionPropertyName;

    public BallotIntegrityCheckerAttribute(string electionPropertyName)
    {
        _electionPropertyName = electionPropertyName;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        ILogger? logger = null;
        if (validationContext.Items.TryGetValue("Logger", out var loggerContextValue) && loggerContextValue is ILogger castedLogger)
        {
            logger = castedLogger;
        }

        var electionPropertyInfo = validationContext.ObjectType.GetProperty(_electionPropertyName);
        if (electionPropertyInfo is null)
        {
            return new ValidationResult($"Election Model property not found.", [validationContext.MemberName]);
        }

        var electionPropertyValue = electionPropertyInfo.GetValue(validationContext.ObjectInstance);
        if (electionPropertyValue is not ElectionModel election)
        {
            return new ValidationResult($"Property '{_electionPropertyName}' is not a valid ElectionModel type.", [validationContext.MemberName]);
        }

        var trueVoteDbContext = (ITrueVoteDbContext?) validationContext.GetService(typeof(ITrueVoteDbContext));
        if (trueVoteDbContext is null)
        {
            if (!validationContext.Items.TryGetValue("DBContext", out var dbContextValue) || dbContextValue is not ITrueVoteDbContext dbContext)
            {
                return new ValidationResult($"Could not get DBContext for Property '{_electionPropertyName}'.", [validationContext.MemberName]);
            }
            trueVoteDbContext = dbContext;
        }

        // TODO Make this async
        var electionFromDBSet = trueVoteDbContext.Elections.Where(e => e.ElectionId == election.ElectionId);
        if (electionFromDBSet.ToList().Count == 0)
        {
            return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Election not found.", [validationContext.MemberName]);
        }

        var electionFromDB = electionFromDBSet.First();
        var now = UtcNowProviderFactory.GetProvider().UtcNow;

        if (now < electionFromDB.StartDate)
        {
            return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Submitted at: {now}, which is before the election start: {electionFromDB.StartDate}.", [validationContext.MemberName]);
        }

        if (now > electionFromDB.EndDate)
        {
            return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Submitted at: {now}, which is after the election end: {electionFromDB.EndDate}.", [validationContext.MemberName]);
        }

        var diff = electionFromDB.ModelDiff(election);

        var validationResults = new List<ValidationResult>();

        foreach (var (key, v) in diff)
        {
            if (!key.Contains(".Selected"))
            {
                logger?.LogInformation($"{key}: Old = {v.OldValue}, New = {v.NewValue}");
                validationResults.Add(new ValidationResult($"Value for {key} changed from {v.OldValue} to {v.NewValue}", [key]));
            }
        }

        if (validationResults.Count > 0)
        {
            var recursiveValidator = new RecursiveValidator();
            var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);
            var errorJson = JsonSerializer.Serialize(errorDictionary);
            return new ValidationResult(errorJson);
        }

        return ValidationResult.Success;
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
