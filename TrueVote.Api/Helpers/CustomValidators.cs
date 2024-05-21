#pragma warning disable IDE0046 // Convert to conditional expression
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TrueVote.Api.Helpers
{
    [ExcludeFromCodeCoverage]
    public class NumberOfChoicesValidatorAttribute : ValidationAttribute
    {
        private readonly string _propertyName;

        public NumberOfChoicesValidatorAttribute(string propertyName)
        {
            _propertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the property info of the property
            var propertyInfo = validationContext.ObjectType.GetProperty(_propertyName);

            if (propertyInfo == null)
            {
                return new ValidationResult($"Property '{_propertyName}' not found.");
            }

            // Get the value of the property

            if (propertyInfo.GetValue(validationContext.ObjectInstance) is not IEnumerable propertyValue)
            {
                return new ValidationResult($"Property '{_propertyName}' is not a valid collection.");
            }

            // Calculate the count of the collection
            var count = 0;
            foreach (var _ in propertyValue)
            {
                count++;
            }

            var numberOfChoices = value as int?;
            if (numberOfChoices.HasValue && numberOfChoices.Value > count)
            {
                return new ValidationResult($"Number of choices cannot exceed the number of {_propertyName}. NumberOfChoices: {numberOfChoices}, {_propertyName}: {count}");
            }

            return ValidationResult.Success;
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
