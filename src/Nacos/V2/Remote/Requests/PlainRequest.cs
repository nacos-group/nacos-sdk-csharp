namespace Nacos.V2.Remote.Requests
{
    public class PlainRequest
    {
        public string Type { get; set; }

        public object Body { get; set; }

        public CommonRequestMeta Metadata { get; set; }
    }
}
