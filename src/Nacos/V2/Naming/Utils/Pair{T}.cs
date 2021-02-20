namespace Nacos.V2.Naming.Utils
{
    public class Pair<T>
    {
        public T Item { get; private set; }

        public double Weight { get; private set; }

        public Pair(T item, double weight)
        {
            this.Item = item;
            this.Weight = weight;
        }
    }
}
