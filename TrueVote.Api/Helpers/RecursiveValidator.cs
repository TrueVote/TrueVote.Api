using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
