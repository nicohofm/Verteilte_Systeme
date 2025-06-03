using Confluent.Kafka;
using KafkaWeather.Helper;
using Newtonsoft.Json;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;

namespace KafkaWeather
{
    class Program
    {
        static void Main(string[] args)
        {
            string kafkaBootstrapServers = "10.50.15.52:9092";
            string kafkaTopic = "weather";
            string groupId = "vlvs_inf22_graphite_consumer_group";
            string graphiteHost = "10.50.15.52";
            string topicName = "vlvs_inf22_mgh_to_ju_ti_ni";

            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            var producer = new CpuLoadProducer(kafkaBootstrapServers, topicName);
            producer.Run(cts.Token);

            using (var consumer = new WeatherConsumer(kafkaBootstrapServers, kafkaTopic, groupId, graphiteHost))
            {
                consumer.Run();
            }
        }
    }
}
