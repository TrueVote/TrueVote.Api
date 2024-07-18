using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

#pragma warning disable IDE0046 // Convert to conditional expression
namespace TrueVote.Api.Helpers;

public interface IRecursiveValidator
{
    bool TryValidateObjectRecursive(object obj, ValidationContext validationContext, List<ValidationResult> results);
    Dictionary<string, string[]> GetValidationErrorsDictionary(List<ValidationResult> results);
}

public class RecursiveValidator : IRecursiveValidator
{
    public bool TryValidateObjectRecursive(object obj, ValidationContext validationContext, List<ValidationResult> results)
    {
        if (obj is null) return true;
        var result = Validator.TryValidateObject(obj, validationContext, results, true);

        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = property.GetValue(obj);
            if (value is null) continue;

            if (value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is not null)
                    {
                        var nestedContext = new ValidationContext(item, validationContext, validationContext.Items);
                        result = TryValidateObjectRecursive(item, nestedContext, results) && result;
                    }
                }
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var nestedContext = new ValidationContext(value, validationContext, validationContext.Items);
                result = TryValidateObjectRecursive(value, nestedContext, results) && result;
            }
        }

        return result;
    }

    public Dictionary<string, string[]> GetValidationErrorsDictionary(List<ValidationResult> results)
    {
        return results
            .SelectMany(vr => vr.MemberNames.Select(memberName => new { memberName, ErrorMessage = vr.ErrorMessage ?? string.Empty }))
            .GroupBy(x => x.memberName, x => x.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

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
        if (candidatePropertyValue is not List<CandidateModel>)
        {
            return new ValidationResult($"Property '{PropertyName}' is not a valid List<CandidateModel> type.", [validationContext.MemberName]);
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

        var selectedCount = ((IEnumerable) candidatePropertyValue).Cast<CandidateModel>().Count(c => c.Selected == true);

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

        var electionFromDBSet = trueVoteDbContext.Elections.Where(e => e.ElectionId == election.ElectionId);

        if (!electionFromDBSet.Any())
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
