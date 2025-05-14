using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace E_CommerceTask.Extentions
{
    public static class ModelStateExtensions
    {
        public static string ExtractErrors(this ModelStateDictionary modelState)
        {
            var errorMessages = modelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            var allErrors = string.Join(" | ", errorMessages);
            return allErrors;
        }
    }
}
