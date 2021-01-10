namespace Nacos.V2
{
    using System.Threading.Tasks;

    public interface IEventListener
    {
        Task OnEvent(IEvent @event);
    }
}