namespace Nacos.Auth.Ram.Injector
{
    using Nacos.Auth.Ram.Utils;
    using Nacos.Utils;

    public class ConfigResourceInjector : AbstractResourceInjector
    {
        private static readonly string ACCESS_KEY_HEADER = "Spas-AccessKey";

        private static readonly string DEFAULT_RESOURCE = "";

        public override void DoInject(RequestResource resource, RamContext context, LoginIdentityContext result)
        {
            var accessKey = context.AccessKey;
            var secretKey = context.SecretKey;

            /* TODO: sts logic */

            if (accessKey.IsNotNullOrWhiteSpace()
                && secretKey.IsNotNullOrWhiteSpace())
            {
                result.SetParameter(ACCESS_KEY_HEADER, accessKey);
            }

            var signHeaders = SpasAdapter.GetSignHeaders(GetResource(resource.Namespace, resource.Group), secretKey);
            result.SetParameter(signHeaders);
        }

        private string GetResource(string tenant, string group)
        {
            if (tenant.IsNotNullOrWhiteSpace()
                && group.IsNotNullOrWhiteSpace())
            {
                return string.Concat(tenant, "+", group);
            }

            if (group.IsNotNullOrWhiteSpace())
            {
                return group;
            }

            if (tenant.IsNotNullOrWhiteSpace())
            {
                return tenant;
            }

            return DEFAULT_RESOURCE;
        }
    }
}
