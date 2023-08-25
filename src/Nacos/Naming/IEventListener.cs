namespace Nacos.Naming
{
    using System.Threading.Tasks;

    public interface IEventListener
    {
        Task OnEvent(IEvent @event);
    }
}