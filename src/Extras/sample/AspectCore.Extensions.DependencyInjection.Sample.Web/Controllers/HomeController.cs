using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Extensions.DependencyInjection.Sample.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspectCore.Extensions.DependencyInjection.Sample.Web.Controllers
{
    [Produces("application/json")]
    [Route("api/Homm1")]
    public class HomeController : Controller
    {
        public string Get([FromServices]IHomeService homeService)
        {
            return "test";
        }
    }
}