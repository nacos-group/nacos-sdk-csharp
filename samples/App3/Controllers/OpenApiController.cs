namespace App3.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Nacos.V2.Utils;
    using System.Threading.Tasks;

    [ApiController]
    [Route("o")]
    public class OpenApiController : ControllerBase
    {
        private readonly Nacos.OpenApi.INacosOpenApi _api;

        public OpenApiController(Nacos.OpenApi.INacosOpenApi api)
        {
            _api = api;
        }

        // GET o/n-g
        [HttpGet("n-g")]
        public async Task<string> NamespaceGetAll()
        {
            var list = await _api.GetNamespacesAsync().ConfigureAwait(false);

            var res = list.ToJsonString();

            return res ?? "GetAllInstances";
        }

        // GET o/n-g
        [HttpGet("n-c")]
        public async Task<string> NamespaceCreate(string i, string n)
        {
            var flag = await _api.CreateNamespaceAsync(i, n, "").ConfigureAwait(false);

            return flag.ToString();
        }

        // GET o/n-u
        [HttpGet("n-u")]
        public async Task<string> NamespaceUpdate(string i, string n)
        {
            var flag = await _api.UpdateNamespaceAsync(i, n, "").ConfigureAwait(false);

            return flag.ToString();
        }

        // GET o/n-u
        [HttpGet("n-d")]
        public async Task<string> NamespaceDelete(string i)
        {
            var flag = await _api.DeleteNamespaceAsync(i).ConfigureAwait(false);

            return flag.ToString();
        }
    }
}
