using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KafkaWeather.Helper
{
    public class GraphiteClient : IDisposable
    {
        private readonly string host;
        private readonly int port;
        private TcpClient tcpClient;
        private NetworkStream stream;

        public GraphiteClient(string host, int port = 2003)
        {
            this.host = host;
            this.port = port;
            Connect();
        }

        private void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            stream = tcpClient.GetStream();
        }

        public void SendMetric(string metricPath, double value, long timestamp)
        {
            string message = $"{metricPath} {value} {timestamp}\n";
            byte[] bytes = Encoding.ASCII.GetBytes(message);

            try
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
            catch (Exception)
            {
                // Reconnect und nochmal senden
                Reconnect();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        private void Reconnect()
        {
            Dispose();
            Connect();
        }

        public void Dispose()
        {
            stream?.Dispose();
            tcpClient?.Close();
        }
    }
}