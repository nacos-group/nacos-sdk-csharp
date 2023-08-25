namespace Nacos.Auth
{
    public class RequestResource
    {
        public RequestResource(string type, string @namespace, string group, string resource)
        {
            Type = type;
            Namespace = @namespace;
            Group = group;
            Resource = resource;
        }

        /// <summary>
        /// Request type: naming or config.
        /// </summary>
        public string Type { get; private set; }

        public string Namespace { get; private set; }

        public string Group { get; private set; }

        /// <summary>
        /// For type: naming, the resource should be service name.
        /// For type: config, the resource should be config dataId.
        /// </summary>
        public string Resource { get; set; }
    }
}
