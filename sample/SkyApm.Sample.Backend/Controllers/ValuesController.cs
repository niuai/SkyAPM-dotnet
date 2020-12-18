using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SkyApm.Sample.Backend.Models;

namespace SkyApm.Sample.Backend.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            await new HttpClient().GetAsync("http://localhost:5003/api/WeatherForecast");

            return new List<string> {"value1", "value2"};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpGet("ignore")]
        public string Ignore()
        {
            return "ignore";
        }

        [HttpGet("StopPropagation")]
        public string StopPropagation()
        {
            return "stop propagation";
        }
    } 
}