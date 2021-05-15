namespace App3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nacos.V2;
    using System.Threading.Tasks;

    [ApiController]
    [Route("c")]
    public class ConfigController : ControllerBase
    {
        private readonly INacosConfigService _svc;

        public ConfigController(INacosConfigService svc)
        {
            _svc = svc;
        }

        // GET c/g?d=123
        [HttpGet("g")]
        public async Task<string> Get(string d)
        {
            var res = await _svc.GetConfig(d, "g", 3000).ConfigureAwait(false);

            return res ?? "empty config";
        }

        // GET c/d?d=123
        [HttpGet("d")]
        public async Task<string> Delete(string d)
        {
            var res = await _svc.RemoveConfig(d, "g").ConfigureAwait(false);

            return "d ok" + res;
        }

        // GET c/p?d=123
        [HttpGet("p")]
        public async Task<string> Publish(string d)
        {
            var res = await _svc.PublishConfig(d, "g", new System.Random().Next(1, 9999999).ToString()).ConfigureAwait(false);

            return "p ok" + res;
        }

        // GET c/al?d=123
        [HttpGet("a")]
        public async Task<string> Listen(string d)
        {
            await _svc.AddListener(d, "g", _configListen).ConfigureAwait(false);
            return "al ok";
        }

        // GET c/rl?d=123
        [HttpGet("r")]
        public async Task<string> UnListen(string d)
        {
            await _svc.RemoveListener(d, "g", _configListen).ConfigureAwait(false);

            return "rl ok";
        }

        private static CusConfigListen _configListen = new CusConfigListen();

        public class CusConfigListen : Nacos.V2.IListener
        {
            public void ReceiveConfigInfo(string configInfo)
            {
                System.Console.WriteLine("config cb cb cb " + configInfo);
            }
        }
    }
}
