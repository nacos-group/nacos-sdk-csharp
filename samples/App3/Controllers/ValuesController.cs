namespace App3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nacos.AspNetCore;
    using System.Collections.Generic;
    using System.Net.Http;

    [ApiController]
    [Route("[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly INacosServerManager _serverManager;

        public ValuesController(INacosServerManager serverManager)
        {
            _serverManager = serverManager;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", "App3" };
        }

        // GET api/values/test
        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            var baseUrl = _serverManager.GetServerAsync("App1").GetAwaiter().GetResult();

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
