using System.Collections;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EarlyWarning.Extensions;

/// <summary>Marks an action that checks its beneficiary tables itself (e.g. only the sections the user answered "yes" to).</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SkipBeneficiaryValidationAttribute : Attribute;

/// <summary>
/// On every POST, finds the beneficiary tables in the bound arguments (directly, in lists, or one level
/// down in a view model) and adds a model error for each distribution that exceeds its primary total.
/// See <see cref="Beneficiaries"/> for the rule.
/// </summary>
public sealed class BeneficiaryValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!HttpMethods.IsPost(context.HttpContext.Request.Method)) return;
        if (context.ActionDescriptor.EndpointMetadata.OfType<SkipBeneficiaryValidationAttribute>().Any()) return;
        foreach (var (name, value) in context.ActionArguments)
            Check(context.ModelState, name, value, depth: 0);
    }

    public void OnActionExecuted(ActionExecutedContext context) { }

    public static void Check(ModelStateDictionary state, string key, object? value, int depth, string? section = null)
    {
        if (value == null || value is string || depth > 2) return;
        if (Beneficiaries.From(value) is { } people)
        {
            foreach (var problem in people.Problems())
                state.AddModelError(key, section == null ? problem : $"{section}፡ {problem}");
            return;
        }
        if (value is IEnumerable list)
        {
            var i = 0;
            foreach (var item in list) Check(state, $"{key}[{i++}]", item, depth + 1, section);
            return;
        }
        // Only walk into this application's own view models, never into EF navigation graphs.
        var type = value.GetType();
        if (depth > 0 || type.Namespace?.StartsWith("EarlyWarning.ViewModels") != true) return;
        foreach (var property in type.GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
            Check(state, property.Name, property.GetValue(value), depth + 1, section);
    }
}
