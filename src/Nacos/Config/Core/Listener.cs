namespace Nacos
{
    using System.Threading;

    public class Listener
    {
        public Listener(string name, CancellationTokenSource cts)
        {
            this.Name = name;
            this.Cts = cts;
        }

        public string Name { get; private set; }

        public CancellationTokenSource Cts { get; set; }
    }
}
