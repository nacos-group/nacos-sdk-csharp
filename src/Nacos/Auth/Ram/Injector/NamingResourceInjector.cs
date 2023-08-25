namespace Nacos.Auth.Ram.Injector
{
    using Nacos.Auth.Ram.Utils;
    using Nacos.Common;
    using Nacos.Naming.Utils;
    using Nacos.Utils;
    using System;

    public class NamingResourceInjector : AbstractResourceInjector
    {
        private static readonly string SIGNATURE_FILED = "signature";

        private static readonly string DATA_FILED = "data";

        private static readonly string AK_FILED = "ak";

        public override void DoInject(RequestResource resource, RamContext context, LoginIdentityContext result)
        {
            if (context.Validate())
            {
                try
                {
                    string accessKey = context.AccessKey;
                    string secretKey = context.SecretKey;

                    /* TODO: sts logic */

                    string signData = GetSignData(GetGroupedServiceName(resource));
                    string signature = SignUtil.Sign(signData, secretKey);
                    result.SetParameter(SIGNATURE_FILED, signature);
                    result.SetParameter(DATA_FILED, signData);
                    result.SetParameter(AK_FILED, accessKey);
                }
                catch (Exception)
                {
                }
            }
        }

        private string GetGroupedServiceName(RequestResource resource)
        {
            if (resource.Resource.Contains(Constants.SERVICE_INFO_SPLITER)
                || resource.Group.IsNullOrWhiteSpace())
            {
                return resource.Resource;
            }

            return NamingUtils.GetGroupedNameOptional(resource.Resource, resource.Group);
        }

        private string GetSignData(string serviceName)
        {
            return serviceName.IsNotNullOrWhiteSpace()
                ? DateTimeOffset.Now.ToUnixTimeMilliseconds() + Constants.SERVICE_INFO_SPLITER + serviceName
                : DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
        }
    }
}
