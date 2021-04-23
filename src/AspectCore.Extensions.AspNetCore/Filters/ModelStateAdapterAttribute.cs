using Microsoft.AspNetCore.Mvc.Filters;

namespace AspectCore.Extensions.AspNetCore.Filters
{
    /// <summary>
    /// 将asp.net core mvc的模型绑定ModelState保存与上下文中
    /// </summary>
    public class ModelStateAdapterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.HttpContext.Items["modelstate-aspectcore"] = context.ModelState;
            base.OnActionExecuting(context);
        }
    }
}