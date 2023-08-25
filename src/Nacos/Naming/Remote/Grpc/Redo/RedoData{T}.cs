namespace Nacos.Naming.Remote.Grpc.Redo
{
    public abstract class RedoData<T>
        where T : class
    {
        protected RedoData(string serviceName, string groupName)
        {
            ServiceName = serviceName;
            GroupName = groupName;
        }

        public string ServiceName { get; private set; }

        public string GroupName { get; private set; }

        public T Data { get; set; }

        public bool Registered { get; set; }

        public bool Unregistering { get; set; }

        public RedoType GetRedoType()
        {
            if (Registered && !Unregistering)
            {
                return RedoType.NONE;
            }
            else if (Registered && Unregistering)
            {
                return RedoType.UNREGISTER;
            }
            else if (!Registered && !Unregistering)
            {
                return RedoType.REGISTER;
            }
            else
            {
                return RedoType.REMOVE;
            }
        }

        public bool IsNeedRedo() => !RedoType.NONE.Equals(GetRedoType());
    }
}
