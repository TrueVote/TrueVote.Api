using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

#pragma warning disable IDE0046 // Convert to conditional expression
namespace TrueVote.Api.Helpers
{
    public static class ModelDiffExtensions
    {
        public static Dictionary<string, (object? OldValue, object? NewValue)> ModelDiff<T>(this T? a, T? b, string prefix = "") where T : class
        {
            return CompareObjects(a, b, prefix);
        }

        internal static Dictionary<string, (object? OldValue, object? NewValue)> CompareObjects(object? a, object? b, string prefix)
        {
            var differences = new Dictionary<string, (object? OldValue, object? NewValue)>();

            if (a == null && b == null)
                return differences;

            if (a == null || b == null)
            {
                differences[prefix.TrimEnd('.')] = (a, b);
                return differences;
            }

            var type = a.GetType();

            if (IsSimpleType(type))
            {
                return CompareSimpleTypes(a, b, prefix);
            }

            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                return CompareEnumerables(a as IEnumerable, b as IEnumerable, prefix);
            }

            return CompareComplexTypes(a, b, prefix);
        }

        internal static Dictionary<string, (object? OldValue, object? NewValue)> CompareSimpleTypes(object a, object b, string prefix)
        {
            var differences = new Dictionary<string, (object? OldValue, object? NewValue)>();

            if (a is DateTime || a is DateTime?)
            {
                if (!AreDateTimesEqual(a as DateTime?, b as DateTime?))
                {
                    differences[prefix.TrimEnd('.')] = (a, b);
                }
            }
            else if (!Equals(a, b))
            {
                differences[prefix.TrimEnd('.')] = (a, b);
            }

            return differences;
        }

        internal static string JoinListItems(IEnumerable<string?> items)
        {
            return string.Join(",", items.Select(i => i ?? string.Empty));
        }

        [ExcludeFromCodeCoverage] // So strange that this needs to be uncovered, but it does.
        public static string CreateKey(string prefix)
        {
            return prefix.TrimEnd('.');
        }

        public static Dictionary<string, (object? OldValue, object? NewValue)> CompareEnumerables(IEnumerable? a, IEnumerable? b, string prefix)
        {
            var differences = new Dictionary<string, (object? OldValue, object? NewValue)>();

            var listA = a?.Cast<object>().ToList() ?? new List<object>();
            var listB = b?.Cast<object>().ToList() ?? new List<object>();

            var maxCount = Math.Max(listA.Count, listB.Count);

            for (var i = 0; i < maxCount; i++)
            {
                var itemA = i < listA.Count ? listA[i] : null;
                var itemB = i < listB.Count ? listB[i] : null;

                if (itemA == null && itemB == null) continue;

                if (itemA == null || itemB == null || !Equals(itemA, itemB))
                {
                    if (itemA != null && itemB != null && !IsSimpleType(itemA.GetType()))
                    {
                        var nestedDifferences = CompareObjects(itemA, itemB, $"{prefix}[{i}].");
                        foreach (var kvp in nestedDifferences)
                        {
                            differences[kvp.Key] = kvp.Value;
                        }
                    }
                    else
                    {
                        var displayA = listA.Select(item => item != null && !IsSimpleType(item.GetType()) ? "ComplexType" : item?.ToString());
                        var displayB = listB.Select(item => item != null && !IsSimpleType(item.GetType()) ? "ComplexType" : item?.ToString());
                        differences[CreateKey(prefix)] = (JoinListItems(displayA), JoinListItems(displayB));
                        return differences;
                    }
                }
            }

            return differences;
        }

        public static Dictionary<string, (object? OldValue, object? NewValue)> CompareComplexTypes(object a, object b, string prefix)
        {
            var differences = new Dictionary<string, (object? OldValue, object? NewValue)>();
            var properties = a.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                object? valueA, valueB;
                try
                {
                    valueA = property.GetValue(a);
                    valueB = property.GetValue(b);
                }
                catch (Exception)
                {
                    continue;
                }

                var propertyName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}{property.Name}";
                var propertyDifferences = CompareObjects(valueA, valueB, propertyName);

                foreach (var kvp in propertyDifferences)
                {
                    differences[kvp.Key] = kvp.Value;
                }
            }

            return differences;
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid)
                || type.IsEnum
                || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool AreDateTimesEqual(DateTime? a, DateTime? b)
        {
            return a == b || (a.HasValue && b.HasValue && a.Value.Date == b.Value.Date &&
                   a.Value.Hour == b.Value.Hour && a.Value.Minute == b.Value.Minute &&
                   a.Value.Second == b.Value.Second);
        }
    }
}
#pragma warning restore IDE0046 // Convert to conditional expression
