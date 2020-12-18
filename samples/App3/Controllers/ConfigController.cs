namespace App3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nacos;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        private readonly INacosConfigClientFactory _factory;

        public ConfigController(INacosConfigClientFactory factory)
        {
            _factory = factory;
        }

        // GET api/config/g?d=123
        [HttpGet("g")]
        public async Task<string> Get(string d)
        {
            var client = _factory.GetConfigClient("grpc");

            var res = await client.GetConfigAsync(new GetConfigRequest { DataId = d, Group = "g", Tenant = "test" });

            return res;
        }

        // GET api/config/d?d=123
        [HttpGet("d")]
        public async Task<string> Delete(string d)
        {
            var client = _factory.GetConfigClient("grpc");

            var res = await client.RemoveConfigAsync(new RemoveConfigRequest { DataId = d, Group = "g", Tenant = "test" });

            return "d ok";
        }

        // GET api/config/p?d=123
        [HttpGet("p")]
        public async Task<string> Publish(string d)
        {
            var client = _factory.GetConfigClient("grpc");

            var res = await client.PublishConfigAsync(new PublishConfigRequest { DataId = d, Group = "g", Tenant = "test", Content = new System.Random().Next(1, 9999999).ToString() });

            return "p ok";
        }

        // GET api/config/l?d=123
        [HttpGet("a")]
        public async Task<string> Listen(string d)
        {
            var client = _factory.GetConfigClient("grpc");

            await client.AddListenerAsync(new AddListenerRequest { DataId = d, Group = "g", Tenant = "test", Content = new System.Random().Next(1, 9999999).ToString() });

            return "p ok";
        }
    }
}
