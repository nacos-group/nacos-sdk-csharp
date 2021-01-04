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
            var list = await _client.GetAllInstances("mysvc2", false);

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
                Port = 9562,
                ServiceName = "mysvc2"
            };

            await _client.RegisterInstance("mysvc2", instance);

            return "RegisterInstance ok";
        }

        // GET n/dr
        [HttpGet("dr")]
        public async Task<string> DeregisterInstance()
        {
            // await _client.RegisterInstance("mysvc", "127.0.0.1", 9635);
            var instance = new Nacos.V2.Naming.Dtos.Instance
            {
                Ip = "127.0.0.1",
                Ephemeral = true,
                Port = 9562,
                ServiceName = "mysvc2"
            };

            await _client.DeregisterInstance("mysvc2", instance);

            return "DeregisterInstance ok";
        }

        // GET n/si
        [HttpGet("si")]
        public async Task<string> SelectInstances()
        {
            var list = await _client.SelectInstances("mysvc2", true, false);

            var res = list.ToJsonString();

            return res ?? "SelectInstances ok";
        }

        // GET n/gs
        [HttpGet("gs")]
        public async Task<string> GetServicesOfServer()
        {
            var list = await _client.GetServicesOfServer(1, 10);

            var res = list.ToJsonString();

            return res ?? "GetServicesOfServer";
        }

        // GET n/sub
        [HttpGet("sub")]
        public async Task<string> Subscribe()
        {
            await _client.Subscribe("mysvc2", listener);
            return "Subscribe";
        }

        // GET n/unsub
        [HttpGet("unsub")]
        public async Task<string> Unsubscribe()
        {
            await _client.Unsubscribe("mysvc2", listener);
            return "Subscribe";
        }

        // NOTE: MUST keep Subscribe and Unsubscribe to use one instance of the listener!!!
        // DO NOT create new instance for each opreation!!!
        private static CusListener listener = new CusListener();

        public class CusListener : Nacos.V2.IEventListener
        {
            public Task OnEvent(Nacos.V2.IEvent @event)
            {
                if (@event is Nacos.V2.Naming.Event.InstancesChangeEvent e)
                {
                    System.Console.WriteLine("CusListener");
                    System.Console.WriteLine("ServiceName" + e.ServiceName);
                    System.Console.WriteLine("GroupName" + e.GroupName);
                    System.Console.WriteLine("Clusters" + e.Clusters);
                    System.Console.WriteLine("Hosts" + e.Hosts.ToJsonString());
                }

                return Task.CompletedTask;
            }
        }
    }
}
