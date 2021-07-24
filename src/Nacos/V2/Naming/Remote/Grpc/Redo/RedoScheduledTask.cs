namespace Nacos.V2.Naming.Remote.Grpc
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Exceptions;
    using System;
    using System.Threading.Tasks;

    public class RedoScheduledTask
    {
        private readonly ILogger _logger;
        private readonly NamingGrpcClientProxy _clientProxy;
        private readonly NamingGrpcRedoService _redoService;

        public RedoScheduledTask(ILogger logger, NamingGrpcClientProxy clientProxy, NamingGrpcRedoService redoService)
        {
            this._logger = logger;
            this._clientProxy = clientProxy;
            this._redoService = redoService;
        }

        public async Task Run()
        {
            if (!_redoService.IsConnected())
            {
                _logger.LogWarning("Grpc Connection is disconnect, skip current redo task");
                return;
            }

            try
            {
                await RedoForInstances().ConfigureAwait(false);
                await RedoForSubscribes().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Grpc Connection is disconnect, skip current redo task");
            }
        }

        private async Task RedoForInstances()
        {
            foreach (var item in _redoService.FindInstanceRedoData())
            {
                try
                {
                    await RedoForInstance(item).ConfigureAwait(false);
                }
                catch (NacosException e)
                {
                    _logger.LogWarning(e, "Redo instance operation {0} for {1}@@{2} failed. ", item.GetRedoType(), item.GroupName, item.ServiceName);
                }
            }
        }

        private async Task RedoForInstance(InstanceRedoData redoData)
        {
            var redoType = redoData.GetRedoType();

            string serviceName = redoData.ServiceName;
            string groupName = redoData.GroupName;

            _logger?.LogInformation("Redo instance operation {0} for {1}@@{2}", redoType, groupName, serviceName);

            switch (redoType)
            {
                case RedoType.REGISTER:
                    if (IsClientDisabled()) return;

                    await _clientProxy.DoRegisterService(serviceName, groupName, redoData.Data).ConfigureAwait(false);
                    break;
                case RedoType.UNREGISTER:
                    if (IsClientDisabled()) return;

                    await _clientProxy.DoDeregisterService(serviceName, groupName, redoData.Data).ConfigureAwait(false);
                    break;
                case RedoType.REMOVE:
                    _redoService.RemoveInstanceForRedo(serviceName, groupName);
                    break;
                default:
                    break;
            }
        }

        private async Task RedoForSubscribes()
        {
            foreach (var item in _redoService.FindSubscriberRedoData())
            {
                try
                {
                    await RedoForSubscribe(item).ConfigureAwait(false);
                }
                catch (NacosException e)
                {
                    _logger.LogWarning(e, "Redo subscriber operation {0} for {1}@@{2} failed. ", item.GetRedoType(), item.GroupName, item.ServiceName);
                }
            }
        }

        private async Task RedoForSubscribe(SubscriberRedoData redoData)
        {
            var redoType = redoData.GetRedoType();
            string serviceName = redoData.ServiceName;
            string groupName = redoData.GroupName;
            string cluster = redoData.Data;

            _logger?.LogInformation("Redo subscriber operation {0} for {1}@@{2}#{3}", redoType, groupName, serviceName, cluster);
            switch (redoType)
            {
                case RedoType.REGISTER:
                    if (IsClientDisabled()) return;

                    await _clientProxy.DoSubscribe(serviceName, groupName, cluster).ConfigureAwait(false);
                    break;
                case RedoType.UNREGISTER:
                    if (IsClientDisabled()) return;

                    await _clientProxy.DoUnsubscribe(serviceName, groupName, cluster).ConfigureAwait(false);
                    break;
                case RedoType.REMOVE:
                    _redoService.RemoveSubscriberForRedo(serviceName, groupName, cluster);
                    break;
                default:
                    break;
            }
        }

        private bool IsClientDisabled() => !_clientProxy.IsEnable();
    }
}
