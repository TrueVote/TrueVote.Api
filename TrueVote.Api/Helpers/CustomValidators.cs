#pragma warning disable IDE0046 // Convert to conditional expression
using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace TrueVote.Api.Helpers
{
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
                return new ValidationResult($"Property not found.", [validationContext.MemberName]);
            }

            // Get the value of the property
            var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);
            if (propertyValue is not IEnumerable or string or int or long or DateTime)
            {
                return new ValidationResult($"Property '{_propertyName}' is not a valid collection.", [validationContext.MemberName]);
            }

            // Calculate the count of the collection
            var count = ((IEnumerable) propertyValue).Cast<object>().Count();

            var numberOfChoices = value as int?;
            if (numberOfChoices.HasValue && numberOfChoices.Value > count)
            {
                return new ValidationResult($"NumberOfChoices cannot exceed the number of items in '{_propertyName}'. NumberOfChoices: {numberOfChoices}, Count: {count}", [validationContext.MemberName]);
            }

            return ValidationResult.Success;
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
