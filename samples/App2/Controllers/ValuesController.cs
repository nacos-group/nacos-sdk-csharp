namespace App2.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Net.Http;

    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // nacos server 1.x and 2.x
        private readonly Nacos.V2.INacosNamingService _svc;

        public ValuesController(Nacos.V2.INacosNamingService svc)
        {
            _svc = svc;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", "App2" };
        }

        // GET api/values/test
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            var instance = _svc.SelectOneHealthyInstance("App1", "DEFAULT_GROUP").GetAwaiter().GetResult();
            var host = $"{instance.Ip}:{instance.Port}";

            var baseUrl = instance.Metadata.TryGetValue("secure", out _)
                ? $"https://{host}"
                : $"http://{host}";

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "empty";
            }

            var url = $"{baseUrl}/api/values";

            using (HttpClient client = new HttpClient())
            {
                var result = client.GetAsync(url).GetAwaiter().GetResult();
                return result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
        }
    }
}
