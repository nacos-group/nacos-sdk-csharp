namespace Nacos.Auth.Ram.Injector
{
    public abstract class AbstractResourceInjector
    {
        public abstract void DoInject(RequestResource resource, RamContext context, LoginIdentityContext result);
    }
}
