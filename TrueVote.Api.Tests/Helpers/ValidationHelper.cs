using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrueVote.Api.Tests.Helpers
{
    public static class ValidationHelper
    {
        public static IList<ValidationResult> Validate(object model)
        {
            var validationResults = new List<ValidationResult>();
            ValidateRecursive(model, validationResults);
            return validationResults;
        }

        private static void ValidateRecursive(object instance, IList<ValidationResult> validationResults)
        {
            if (instance == null)
                return;

            var context = new ValidationContext(instance);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(instance, context, results, true);
            foreach (var validationResult in results)
            {
                validationResults.Add(validationResult);
            }

            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                var propertyValue = property.GetValue(instance);
                if (propertyValue == null)
                    continue;

                // Check if property is a collection or complex type
                if (IsCollection(propertyValue) || IsComplexType(propertyValue))
                {
                    if (IsCollection(propertyValue))
                    {
                        if (propertyValue is IEnumerable enumerable)
                        {
                            foreach (var item in enumerable)
                            {
                                if (item != null)
                                {
                                    ValidateRecursive(item, validationResults);
                                }
                            }
                        }
                    }
                    else
                    {
                        ValidateRecursive(propertyValue, validationResults);
                    }
                }
            }
        }

        private static bool IsCollection(object obj)
        {
            return obj is IEnumerable and not string;
        }

        private static bool IsComplexType(object obj)
        {
            return !obj.GetType().IsValueType && obj.GetType() != typeof(string);
        }
    }
}
