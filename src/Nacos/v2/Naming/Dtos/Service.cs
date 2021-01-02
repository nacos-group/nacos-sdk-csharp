namespace Nacos.V2.Naming.Dtos
{
    using System.Collections.Generic;

    public class Service
    {
        public string Name { get; set; }

        public float ProtectThreshold { get; set; } = 0.0F;


        public string AppName { get; set; }


        public string GroupName { get; set; }

        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
