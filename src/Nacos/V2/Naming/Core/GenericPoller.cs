namespace Nacos.V2.Naming.Core
{
    using System.Collections.Generic;

    public class GenericPoller<T> : IPoller<T>
    {
        private int index = 0;

        private List<T> items = new List<T>();

        public GenericPoller(List<T> items)
        {
            this.items = items;
        }

        public T Next()
        {
            System.Threading.Interlocked.Increment(ref index);

            return items[System.Math.Abs(index % items.Count)];
        }

        public IPoller<T> Refresh(List<T> items)
        {
            return new GenericPoller<T>(items);
        }
    }
}
