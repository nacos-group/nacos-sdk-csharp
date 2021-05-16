namespace Nacos
{
    using Microsoft.Extensions.Logging;
    using Nacos.Utilities;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class PushReceiver : IDisposable
    {
        private readonly ILogger _logger;
        private readonly HostReactor _hostReactor;

        private UdpClient _udpClient;
        private int _port;
        private bool _closed = false;

        public PushReceiver(ILoggerFactory loggerFactory, HostReactor hostReactor)
        {
            _logger = loggerFactory.CreateLogger<PushReceiver>();
            _hostReactor = hostReactor;
            Task.Factory.StartNew(
                async () => await RunAsync().ConfigureAwait(false), TaskCreationOptions.LongRunning);
        }

        public int GetUdpPort()
        {
            return _port;
        }

        private async Task RunAsync()
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
                    _logger?.LogError(ex, "failed to start udp server {0}", i + 1);
                }
            }

            while (!_closed)
            {
                try
                {
                    var res = await _udpClient.ReceiveAsync().ConfigureAwait(false);

                    var json = Encoding.UTF8.GetString(TryDecompressData(res.Buffer));
                    _logger?.LogInformation("received push data: {0} from {1}", json, res.RemoteEndPoint.ToString());

                    var pushPacket = json.ToObj<PushPacket>();

                    var ack = string.Empty;

                    if (pushPacket.Type.Equals("dom", StringComparison.OrdinalIgnoreCase) || pushPacket.Type.Equals("service", StringComparison.OrdinalIgnoreCase))
                    {
                        _hostReactor.ProcessServiceJson(pushPacket.Data);
                        ack = new { type = "push-ack", lastRefTime = pushPacket.LastRefTime, data = "" }.ToJsonString();
                    }
                    else if (pushPacket.Type.Equals("dump", StringComparison.OrdinalIgnoreCase))
                    {
                        var map = _hostReactor.GetServiceInfoMap().ToJsonString();
                        ack = new { type = "dump-ack", lastRefTime = pushPacket.LastRefTime, data = map }.ToJsonString();
                    }
                    else
                    {
                        ack = new { type = "unknown-ack", lastRefTime = pushPacket.LastRefTime, data = "" }.ToJsonString();
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