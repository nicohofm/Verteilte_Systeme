using Confluent.Kafka;
using System;
using System.Threading;

namespace KafkaWeather.Helper
{
    public class CpuLoadProducer
    {
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly Random _rand;
        private readonly IProducer<Null, string> _producer;

        public CpuLoadProducer(string bootstrapServers, string topic)
        {
            _bootstrapServers = bootstrapServers;
            _topic = topic;
            _rand = new Random();

            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers
            };

            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public void Run(CancellationToken token)
        {
            Console.WriteLine($"Producer gestartet – Topic: {_topic}");

            while (!token.IsCancellationRequested)
            {
                double cpuLoad = Math.Round(_rand.NextDouble() * 100, 2);
                string timestamp = DateTime.UtcNow.ToString("o");
                string message = $"{{\"cpuLoad\":{cpuLoad},\"timestamp\":\"{timestamp}\"}}";

                try
                {
                    _producer.Produce(_topic, new Message<Null, string> { Value = message }, report =>
                    {
                        if (report.Error.IsError)
                        {
                            Console.WriteLine($"Fehler: {report.Error.Reason}");
                        }
                        else
                        {
                            Console.WriteLine($"Gesendet: {message}");
                        }
                    });
                }
                catch (ProduceException<Null, string> ex)
                {
                    Console.WriteLine($"Produce-Fehler: {ex.Error.Reason}");
                }

                Thread.Sleep(5000); // Alle 5 Sekunden neue Nachricht
            }

            _producer.Flush(TimeSpan.FromSeconds(5));
            Console.WriteLine("Producer beendet.");
        }
    }
}