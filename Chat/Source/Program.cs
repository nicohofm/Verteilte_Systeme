using Chat.ChatModel;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Chat
{
    internal class Program
    {
        static string clientId = $"chatclient-{Guid.NewGuid().ToString().Substring(0, 8)}";
        static string sender = "123";
        static string topicChat = "/aichat/default";
        static string topicState = "/aichat/clientstate";
        static IMqttClient client;
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithClientId(clientId)
                .WithTcpServer("10.50.12.150", 1883)
                .WithCleanSession()
                .WithWillMessage(new MqttApplicationMessage
                {
                    Topic = topicState,
                    Payload = Encoding.UTF8.GetBytes($"{clientId} stopped."),
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = false
                })
                .Build();

            client.UseConnectedHandler(async e =>
            {
                Console.WriteLine("Verbunden mit dem Broker.");
                await client.SubscribeAsync(topicChat);
                Console.WriteLine($"Abonniert: {topicChat}");

                var startMessage = new MqttApplicationMessage
                {
                    Topic = topicState,
                    Payload = Encoding.UTF8.GetBytes($"{clientId} started."),
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = false
                };

                await client.PublishAsync(startMessage);
            });

            client.UseApplicationMessageReceivedHandler(e =>
            {
                var payloadString = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var msg = JsonSerializer.Deserialize<ChatMessage>(payloadString);

                if (msg?.clientId != clientId)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n[{msg.topic + " " + msg.sender + " " + msg.clientId}] {msg.text}");
                    Console.ResetColor();
                }
            });

            await client.ConnectAsync(options, CancellationToken.None);

            // Eingabeschleife
            while (true)
            {
                Console.Write("\nDu: ");
                var input = Console.ReadLine();

                if (input?.ToLower() == "/exit")
                    break;

                var chatMsg = new ChatMessage
                {
                    sender = sender,
                    text = input ?? "",
                    clientId = clientId,
                    topic = topicChat
                };

                string json = JsonSerializer.Serialize(chatMsg);
                var msg = new MqttApplicationMessage
                {
                    Topic = topicChat,
                    Payload = Encoding.UTF8.GetBytes(json),
                    QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce,
                    ContentType = "application/json"
                };

                await client.PublishAsync(msg);
            }

            await client.DisconnectAsync();
            Console.WriteLine("Client wurde sauber getrennt.");
        }
    }
}
