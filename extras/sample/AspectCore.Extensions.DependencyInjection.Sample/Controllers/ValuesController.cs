using System.Collections.Generic;
using AspectCoreExtensions.DependencyInjection.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspectCore.Extensions.DependencyInjection.Sample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly IValuesService _valueService;

        public ValuesController(IValuesService valuesService)
        {
            _valueService = valuesService;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return ApiResult(() => _valueService.GetAll());
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
