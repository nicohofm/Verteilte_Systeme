using Confluent.Kafka;
using KafkaWeather.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaWeather.Helper
{
    // 3. Kafka Consumer der Wetterdaten ausliest und an Graphite sendet
    public class WeatherConsumer : IDisposable
    {
        private readonly IConsumer<Ignore, string> consumer;
        private readonly GraphiteClient graphiteClient;
        private readonly string kafkaTopic;
        private readonly string metricPrefix;

        public WeatherConsumer(string kafkaBootstrapServers, string kafkaTopic, string groupId, string graphiteHost, int graphitePort = 2003, string metricPrefix = "vlvs_inf22.weather")
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = kafkaBootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            this.kafkaTopic = kafkaTopic;
            this.metricPrefix = metricPrefix;
            graphiteClient = new GraphiteClient(graphiteHost, graphitePort);

            consumer.Subscribe(kafkaTopic);
        }

        public void Run()
        {
            Console.WriteLine("Kafka Consumer für Wetterdaten gestartet, verbinde mit Graphite...");

            bool running = true;
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                running = false;
            };

            while (running)
            {
                try
                {
                    var cr = consumer.Consume(TimeSpan.FromSeconds(1));
                    if (cr == null) continue;

                    string json = cr.Message.Value;

                    WeatherData data;
                    try
                    {
                        data = JsonConvert.DeserializeObject<WeatherData>(json);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"JSON-Parse-Fehler: {ex.Message}");
                        continue;
                    }

                    if (data == null)
                    {
                        Console.WriteLine("Leere Wetterdaten empfangen.");
                        continue;
                    }

                    long ts = ((DateTimeOffset)data.TimeStamp).ToUnixTimeSeconds();

                    // Beispiel: nur aktuelle Temperatur senden
                    string tempMetric = $"{metricPrefix}.{data.City}.tempCurrent";
                    graphiteClient.SendMetric(tempMetric, data.TempCurrent, ts);

                    // Optional: Max/Min Temperatur senden
                    string tempMaxMetric = $"{metricPrefix}.{data.City}.tempMax";
                    graphiteClient.SendMetric(tempMaxMetric, data.TempMax, ts);

                    string tempMinMetric = $"{metricPrefix}.{data.City}.tempMin";
                    graphiteClient.SendMetric(tempMinMetric, data.TempMin, ts);

                    Console.WriteLine($"Wetterdaten für {data.City} gesendet: TempCurrent={data.TempCurrent}°C, TempMax={data.TempMax}°C, TempMin={data.TempMin}°C");
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Consume-Fehler: {e.Error.Reason}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Fehler: {e.Message}");
                }
            }

            consumer.Close();
            Console.WriteLine("Kafka Consumer geschlossen.");
        }

        public void Dispose()
        {
            graphiteClient?.Dispose();
            consumer?.Dispose();
        }
    }
}
