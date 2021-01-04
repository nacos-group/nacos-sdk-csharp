namespace App3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nacos;
    using Nacos.Utilities;
    using System.Threading.Tasks;

    [ApiController]
    [Route("n")]
    public class NamingController : ControllerBase
    {
        private readonly Nacos.V2.INacosNamingService _client;

        public NamingController(Nacos.V2.INacosNamingService client)
        {
            _client = client;
        }

        // GET n/g
        [HttpGet("g")]
        public async Task<string> GetAllInstances()
        {
            var list = await _client.GetAllInstances("mysvc2");

            var res = list.ToJsonString();

            return res ?? "GetAllInstances";
        }

        // GET n/r
        [HttpGet("r")]
        public async Task<string> RegisterInstance()
        {
            // await _client.RegisterInstance("mysvc", "127.0.0.1", 9635);
            var instance = new Nacos.V2.Naming.Dtos.Instance
            {
                Ip = "127.0.0.1",
                Ephemeral = true,
                Port = new System.Random().Next(8000, 9000),
                ServiceName = "mysvc2"
            };

            await _client.RegisterInstance("mysvc2", instance);

            return "RegisterInstance ok";
        }

        // GET /config/p?d=123
        [HttpGet("p")]
        public async Task<string> Publish(string d)
        {
            var list = await _client.GetServicesOfServer(1, 10);

            var res = list.ToJsonString();

            return res ?? "GetServicesOfServer";
        }
    }
}
