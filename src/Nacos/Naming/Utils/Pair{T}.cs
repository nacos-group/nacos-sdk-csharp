namespace Nacos.Naming.Utils
{
    public class Pair<T>
    {
        public T Item { get; private set; }

        public double Weight { get; private set; }

        public Pair(T item, double weight)
        {
            Item = item;
            Weight = weight;
        }
    }
}
