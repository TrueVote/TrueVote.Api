using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TrueVote.Api.Tests.Helpers
{
    public static class ValidationHelper
    {
        public static IList<ValidationResult> Validate(object model, bool isBallot = false)
        {
            var validationResults = new List<ValidationResult>();
            ValidateRecursive(model, validationResults, isBallot);
            return validationResults;
        }

        private static void ValidateRecursive(object instance, IList<ValidationResult> validationResults, bool isBallot)
        {
            if (instance == null)
                return;

            var context = new ValidationContext(instance);
            if (isBallot)
            {
                context.Items["IsBallot"] = true;
            }
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(instance, context, results, true);
            foreach (var validationResult in results)
            {
                validationResults.Add(validationResult);
            }

            var properties = instance.GetType().GetProperties();
            foreach (var property in properties)
            {
                object propertyValue = null;

                try
                {
                    propertyValue = property.GetValue(instance);
                }
                catch (Exception ex)
                {
                    // Log or handle the exception if necessary
                    Console.WriteLine($"Exception when getting value of property '{property.Name}': {ex.Message}");
                    continue; // Skip this property and move to the next one
                }

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
                                    ValidateRecursive(item, validationResults, isBallot);
                                }
                            }
                        }
                    }
                    else
                    {
                        ValidateRecursive(propertyValue, validationResults, isBallot);
                    }
                }
            }
        }

        private static bool IsCollection(object obj)
        {
            return obj is IEnumerable && obj.GetType() != typeof(string);
        }

        private static bool IsComplexType(object obj)
        {
            return !obj.GetType().IsValueType && obj.GetType() != typeof(string);
        }
    }
}
