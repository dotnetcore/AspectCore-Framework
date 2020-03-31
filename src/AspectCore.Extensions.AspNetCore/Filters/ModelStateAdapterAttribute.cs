using Microsoft.AspNetCore.Mvc.Filters;

namespace AspectCore.Extensions.AspNetCore.Filters
{
    public class ModelStateAdapterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["modelstate-aspectcore"] = context.ModelState;
            base.OnActionExecuting(context);
        }
    }
}