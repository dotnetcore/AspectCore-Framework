using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace AspectCore.Extensions.DependencyInjection.Sample.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public virtual IActionResult ApiResult(Func<IActionResult> func)
        {
            return func();
        }
    }
}