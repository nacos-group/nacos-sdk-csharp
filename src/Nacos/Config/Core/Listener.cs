namespace Nacos
{
    using System.Threading;

    public class Listener
    {
        public Listener(string name, Timer timer)
        {
            this.Name = name;
            this.Timer = timer;
        }

        public Listener()
        {
        }

        public string Name { get; private set; }

        public Timer Timer { get; set; }
    }
}
