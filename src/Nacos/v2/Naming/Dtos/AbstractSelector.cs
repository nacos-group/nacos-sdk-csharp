namespace Nacos.V2.Naming.Dtos
{
    public abstract class AbstractSelector
    {
        public string Type { get; private set; }

        public AbstractSelector(string type) => this.Type = type;
    }
}
