namespace Nacos.V2.Naming.Core
{
    using Microsoft.Extensions.Logging;
    using Nacos.V2.Common;
    using Nacos.V2.Naming.Cache;
    using Nacos.V2.Utils;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class PushReceiver : IDisposable
    {
        private static readonly string PUSH_PACKAGE_TYPE_DOM = "dom";
        private static readonly string PUSH_PACKAGE_TYPE_SERVICE = "service";
        private static readonly string PUSH_PACKAGE_TYPE_DUMP = "dump";

        private readonly ILogger _logger;
        private ServiceInfoHolder _serviceInfoHolder;
        private UdpClient _udpClient;
        private int _port;
        private bool _closed = false;

        public PushReceiver(ILogger logger, ServiceInfoHolder serviceInfoHolder, NacosSdkOptions options)
        {
            this._logger = logger;
            this._serviceInfoHolder = serviceInfoHolder;

            // if using grpc, do not setup a udp client here.
            if (!options.NamingUseRpc)
            {
                Task.Factory.StartNew(
                   async () => await RunAsync().ConfigureAwait(false), TaskCreationOptions.LongRunning);
            }
        }

        public int GetUdpPort() => _port;

        private string GetPushReceiverUdpPort() => EnvUtil.GetEnvValue(PropertyKeyConst.PUSH_RECEIVER_UDP_PORT);

        private async Task RunAsync()
        {
            string udpPort = GetPushReceiverUdpPort();

            if (udpPort.IsNotNullOrWhiteSpace())
            {
                _port = Convert.ToInt32(udpPort);
                _udpClient = new UdpClient(_port);
                _logger?.LogInformation($"start up udp server....., port: {_port}");
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        _port = new Random((int)DateTimeOffset.Now.ToUnixTimeSeconds()).Next(0, 1000) + 54951;
                        _udpClient = new UdpClient(_port);
                        _logger?.LogInformation($"start up udp server....., port: {_port}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "failed to start udp server {0}, {1}", i + 1, _port);
                    }
                }
            }

            while (!_closed)
            {
                try
                {
                    if (_udpClient == null)
                    {
                        _closed = true;
                        break;
                    }

                    var res = await _udpClient.ReceiveAsync().ConfigureAwait(false);

                    var json = Encoding.UTF8.GetString(TryDecompressData(res.Buffer));
                    _logger?.LogInformation("received push data: {0} from {1}", json, res.RemoteEndPoint.ToString());

                    var pushPacket = json.ToObj<PushPacket>();

                    var ack = string.Empty;

                    if (pushPacket.Type.Equals(PUSH_PACKAGE_TYPE_DOM, StringComparison.OrdinalIgnoreCase) || pushPacket.Type.Equals(PUSH_PACKAGE_TYPE_SERVICE, StringComparison.OrdinalIgnoreCase))
                    {
                        _serviceInfoHolder.ProcessServiceInfo(pushPacket.Data);

                        ack = new { type = "push-ack", lastRefTime = pushPacket.LastRefTime, data = string.Empty }.ToJsonString();
                    }
                    else if (pushPacket.Type.Equals(PUSH_PACKAGE_TYPE_DUMP, StringComparison.OrdinalIgnoreCase))
                    {
                        var map = System.Net.WebUtility.UrlEncode(_serviceInfoHolder.GetServiceInfoMap().ToJsonString());
                        ack = new { type = "dump-ack", lastRefTime = pushPacket.LastRefTime, data = map }.ToJsonString();
                    }
                    else
                    {
                        ack = new { type = "unknown-ack", lastRefTime = pushPacket.LastRefTime, data = string.Empty }.ToJsonString();
                    }

                    var ackByte = Encoding.UTF8.GetBytes(ack);
                    await _udpClient.SendAsync(ackByte, ackByte.Length, res.RemoteEndPoint).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[NA] error while receiving push data");
                }
            }
        }

        private byte[] TryDecompressData(byte[] data)
        {
            if (!IsGzipFile(data)) return data;

            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            zipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        private bool IsGzipFile(byte[] data)
        {
            if (data == null || data.Length < 2) return false;

            return data[0] == 0x1F && data[1] == 0x8B;
        }

        public void Dispose()
        {
            _closed = true;
            _udpClient?.Close();
            _udpClient?.Dispose();
        }

        public class PushPacket
        {
            public string Type { get; set; }

            public long LastRefTime { get; set; }

            public string Data { get; set; }
        }
    }
}
