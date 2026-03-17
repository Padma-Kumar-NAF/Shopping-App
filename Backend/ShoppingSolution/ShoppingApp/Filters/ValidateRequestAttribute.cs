using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShoppingApp.Filters
{
    public class ValidateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
                return;
            }

            if (context.ActionArguments.Values.Any(v => v == null))
            {
                context.Result = new BadRequestObjectResult("Request body cannot be null");
                return;
            }

            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null)
                {
                    context.Result = new BadRequestObjectResult("Invalid request");
                    return;
                }
            }
        }
    }
}