using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTT.MQTTModel;
using MQTTnet.Client;
using System.Threading;
using MQTTnet.Formatter;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using MQTTnet.Client.Connecting;
using System.Text.Json;

namespace MQTT
{
    internal class Program
    {
        static WeatherModel weatherModel = new WeatherModel();
        static void Main(string[] args)
        {
            Task.Run(() => StartMqttClient()).Wait();
            Console.ReadLine(); // Anwendung offen halten
        }

        static async Task StartMqttClient()
        {
            
            try
            {
                var factory = new MqttFactory();
                var client = factory.CreateMqttClient();

                string topic = "";
                byte[] payload = null;

                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer("10.50.12.150", 1883)
                    .Build();

                var connecctResult = await client.ConnectAsync(options);

                if (connecctResult.ResultCode == MqttClientConnectResultCode.Success)
                {
                    Console.WriteLine("Connected to MQTT broker successfully.");

                    // Subscribe to a topic
                    await client.SubscribeAsync("/weather/mosbach");

                    // Callback function when a message is received
                    client.UseApplicationMessageReceivedHandler(e =>
                    {
                        string json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                        Console.WriteLine($"Received message: {json}");

                        WeatherModel data = JsonSerializer.Deserialize<WeatherModel>(json);

                        Console.WriteLine($"Aktuelle Temperatur: {data.tempCurrent}");
                        Console.WriteLine($"Max Temp: {data.tempMax}");
                        Console.WriteLine($"Min Temp: {data.tempMin}");
                        Console.WriteLine($"Kommentar: {data.comment}");
                        Console.WriteLine($"Zeitstempel: {data.timeStamp}");
                        Console.WriteLine($"Stadt: {data.city}");
                        Console.WriteLine($"Stadt ID: {data.cityId}");

                        return Task.CompletedTask;
                    });

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);    
                throw;
            }

            Console.WriteLine(weatherModel);
        }
    }
}
