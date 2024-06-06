#pragma warning disable IDE0046 // Convert to conditional expression
using System.Collections;
using System.ComponentModel.DataAnnotations;
using TrueVote.Api.Models;

namespace TrueVote.Api.Helpers
{
    public abstract class NumberOfChoicesValidatorAttribute : ValidationAttribute
    {
        protected readonly string CandidatesPropertyName;

        protected NumberOfChoicesValidatorAttribute(string propertyName)
        {
            CandidatesPropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the property info of the property
            var candidatePropertyInfo = validationContext.ObjectType.GetProperty(CandidatesPropertyName);

            if (candidatePropertyInfo == null)
            {
                return new ValidationResult($"Property not found.", [validationContext.MemberName]);
            }

            // Get the value of the property
            var candidatePropertyValue = candidatePropertyInfo.GetValue(validationContext.ObjectInstance);
            if (candidatePropertyValue is not List<CandidateModel>)
            {
                return new ValidationResult($"Property '{CandidatesPropertyName}' is not a valid List<CandidateModel> type.", [validationContext.MemberName]);
            }

            // If any of selections are null, then don't validate. This indicates a property check on an unfilled model.
            var nullCount = ((IEnumerable) candidatePropertyValue).Cast<CandidateModel>().Where(c => c.Selected == null).Count();
            if (nullCount > 0)
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
        public MaxNumberOfChoicesValidatorAttribute(string propertyName) : base(propertyName)
        {
        }

        protected override ValidationResult ValidateCount(object value, int selectedCount, ValidationContext validationContext)
        {
            var maxNumberOfChoices = value as int?;
            if (maxNumberOfChoices.HasValue && (selectedCount > maxNumberOfChoices.Value))
            {
                return new ValidationResult($"Number of selected items in '{CandidatesPropertyName}' cannot exceed MaxNumberOfChoices. MaxNumberOfChoices: {maxNumberOfChoices}, SelectedCount: {selectedCount}", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }

    public class MinNumberOfChoicesValidatorAttribute : NumberOfChoicesValidatorAttribute
    {
        public MinNumberOfChoicesValidatorAttribute(string propertyName) : base(propertyName)
        {
        }

        protected override ValidationResult ValidateCount(object value, int selectedCount, ValidationContext validationContext)
        {
            var minNumberOfChoices = value as int?;
            if (minNumberOfChoices.HasValue && (selectedCount < minNumberOfChoices.Value))
            {
                return new ValidationResult($"Number of selected items in '{CandidatesPropertyName}' must be greater or equal to MinNumberOfChoices. MinNumberOfChoices: {minNumberOfChoices}, Count: {selectedCount}", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
