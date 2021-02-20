namespace Nacos.V2.Naming.Core
{
    using System.Collections.Generic;

    public interface IPoller<T>
    {
        T Next();

        IPoller<T> Refresh(List<T> items);
    }
}
