using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using TrueVote.Api.Interfaces;
using TrueVote.Api.Models;

#pragma warning disable IDE0046 // Convert to conditional expression
#pragma warning disable IDE0047 // Remove unnecessary parentheses
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable IDE0045 // Convert to conditional expression
namespace TrueVote.Api.Helpers
{
    public interface IRecursiveValidator
    {
        bool TryValidateObjectRecursive(object obj, ValidationContext validationContext, List<ValidationResult> results);
        Dictionary<string, string[]> GetValidationErrorsDictionary(List<ValidationResult> results);
    }

    public class RecursiveValidator : IRecursiveValidator
    {
        public bool TryValidateObjectRecursive(object obj, ValidationContext validationContext, List<ValidationResult> results)
        {
            if (obj == null) return true;
            var result = Validator.TryValidateObject(obj, validationContext, results, true);

            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = property.GetValue(obj);
                if (value == null) continue;

                // Check if the property is a collection
                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
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

        protected NumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName)
        {
            PropertyName = propertyName;
            RacePropertyName = racePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the property info of the property
            var propertyInfo = validationContext.ObjectType.GetProperty(PropertyName);
            if (propertyInfo == null)
            {
                return new ValidationResult($"Property not found.", [validationContext.MemberName]);
            }

            // Get the value of the property
            var candidatePropertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);
            if (candidatePropertyValue is not List<CandidateModel>)
            {
                return new ValidationResult($"Property '{PropertyName}' is not a valid List<CandidateModel> type.", [validationContext.MemberName]);
            }

            // Get the value of the Name Property
            var nameProperty = validationContext.ObjectType.GetProperty(RacePropertyName);
            if (nameProperty != null)
            {
                RacePropertyValue = nameProperty.GetValue(validationContext.ObjectInstance).ToString();
            }

            // If not a Ballot, then no need to validate. Get out.
            // This isn't the best way of doing this. It's a bit of a hack. The issue is that we want to use the Election model
            // both for creating an election and ballot submission. This is optimal for model re-use. But the data annotations
            // work for ballot submission, are too tight for election creation. So this flag gets around that. When creating an election
            // if the "IsBallot" flag isn't set, the validator simply allows the validation to proceed.
            if (!validationContext.Items.TryGetValue("IsBallot", out var isBallotObj) || isBallotObj is not bool isBallot || !isBallot)
            {
                return ValidationResult.Success;
            }

            // Calculate the number of selections in the candidate choices
            var selectedCount = ((IEnumerable) candidatePropertyValue).Cast<CandidateModel>().Where(c => c.Selected == true).Count();

            return ValidateCount(value, selectedCount, validationContext);
        }

        protected abstract ValidationResult ValidateCount(object value, int count, ValidationContext validationContext);
    }

    public class MaxNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
    {
        public MaxNumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName) : base(propertyName, racePropertyName)
        {
        }

        protected override ValidationResult ValidateCount(object value, int selectedCount, ValidationContext validationContext)
        {
            var maxNumberOfChoices = value as int?;
            if (maxNumberOfChoices.HasValue && (selectedCount > maxNumberOfChoices.Value))
            {
                return new ValidationResult($"Number of selected items in '{PropertyName}' cannot exceed MaxNumberOfChoices for '{RacePropertyValue}'. MaxNumberOfChoices: {maxNumberOfChoices}, SelectedCount: {selectedCount}", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }

    public class MinNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
    {
        public MinNumberOfChoicesValidatorAttribute(string propertyName, string racePropertyName) : base(propertyName, racePropertyName)
        {
        }

        protected override ValidationResult ValidateCount(object value, int selectedCount, ValidationContext validationContext)
        {
            var minNumberOfChoices = value as int?;
            if (minNumberOfChoices.HasValue && (selectedCount < minNumberOfChoices.Value))
            {
                return new ValidationResult($"Number of selected items in '{PropertyName}' must be greater or equal to MinNumberOfChoices for '{RacePropertyValue}'. MinNumberOfChoices: {minNumberOfChoices}, Count: {selectedCount}", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }

    public class BallotIntegrityCheckerAttribute : ValidationAttribute
    {
        protected readonly string _electionPropertyName;

        public BallotIntegrityCheckerAttribute(string electionPropertyName)
        {
            _electionPropertyName = electionPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the logger
            ILogger? logger = null;
            if (validationContext.Items.TryGetValue("Logger", out var loggerContextValue) && loggerContextValue is ILogger castedLogger)
            {
                logger = castedLogger;
            }

            var electionPropertyInfo = validationContext.ObjectType.GetProperty(_electionPropertyName);
            if (electionPropertyInfo == null)
            {
                return new ValidationResult($"Election Model property not found.", [validationContext.MemberName]);
            }

            // Get the value of the property
            var electionPropertyValue = electionPropertyInfo.GetValue(validationContext.ObjectInstance);
            if (electionPropertyValue is not ElectionModel)
            {
                return new ValidationResult($"Property '{_electionPropertyName}' is not a valid ElectionModel type.", [validationContext.MemberName]);
            }
            var election = (ElectionModel) electionPropertyValue;

            // Get the DB Context
            var trueVoteDbContext = (ITrueVoteDbContext) validationContext.GetService(typeof(ITrueVoteDbContext));
            if (trueVoteDbContext == null)
            {
                // Try and get it from the Items context.
                var dbContext = validationContext.Items.TryGetValue("DBContext", out var dbContextValue) ? dbContextValue : null;
                if (dbContext == null)
                {
                    return new ValidationResult($"Could not get DBContext for Property '{_electionPropertyName}'.", [validationContext.MemberName]);
                }

                trueVoteDbContext = dbContext as ITrueVoteDbContext;
            }

            // Try and get the election from the DB
            var electionFromDBSet = trueVoteDbContext.Elections.Where(e => e.ElectionId == election.ElectionId);

            // See if the Election exists
            var electionCount = electionFromDBSet.Count();
            if (electionCount == 0)
            {
                return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Election not found.", [validationContext.MemberName]);
            }

            // Confirm ballot is within election start / end date.
            var electionFromDB = electionFromDBSet.FirstOrDefault();
            var now = UtcNowProviderFactory.GetProvider().UtcNow;

            var ballotAfterElectionStartDate = now >= electionFromDB.StartDate;
            if (!ballotAfterElectionStartDate)
            {
                return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Submitted at: {now}, which is before the election start: {electionFromDB.StartDate}.", [validationContext.MemberName]);
            }

            var ballotBeforeElectionEndDate =  now <= electionFromDB.EndDate;
            if (!ballotBeforeElectionEndDate)
            {
                return new ValidationResult($"Ballot for Election: {election.ElectionId} is invalid. Submitted at: {now}, which is after the election end: {electionFromDB.EndDate}.", [validationContext.MemberName]);
            }

            // Confirm the selection flag is set on all candidates.
            //var nullCandidates = election.Races.Where(r => r != null).SelectMany(r => r.Candidates).Where(c => c != null && c.Selected == null).ToList();
            //if (nullCandidates.Any())
            //{
            //    // Build a string of Candidate Names
            //    var candidateNames = string.Join(", ", nullCandidates.Select(c => c.Name));

            //    return new ValidationResult($"Ballot contains {nullCandidates.Count} null candidate selections. They must all be true or false. These candidates have null selections: {candidateNames}", [validationContext.MemberName]);
            //}

            // Model diff between ballot passed in and DB election. They should be the same other than 'Selected' property
            var diff = electionFromDB.ModelDiff(election);

            var validationResults = new List<ValidationResult>();

            foreach (var kvp in diff)
            {
                if (!kvp.Key.Contains(".Selected"))
                {
                    logger?.LogInformation($"{kvp.Key}: Old = {kvp.Value.OldValue}, New = {kvp.Value.NewValue}");
                    validationResults.Add(new ValidationResult($"Value for {kvp.Key} changed from {kvp.Value.OldValue} to {kvp.Value.NewValue}", [kvp.Key]));
                }
            }

            if (validationResults.Count > 0)
            {
                var recursiveValidator = new RecursiveValidator();

                var errorDictionary = recursiveValidator.GetValidationErrorsDictionary(validationResults);

                var errorJson = JsonSerializer.Serialize(errorDictionary, (JsonSerializerOptions) null);

                return new ValidationResult(errorJson);
            }

            return ValidationResult.Success;
        }
    }
}
#pragma warning restore IDE0045 // Convert to conditional expression
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore IDE0047 // Remove unnecessary parentheses
#pragma warning restore IDE0046 // Convert to conditional expression
