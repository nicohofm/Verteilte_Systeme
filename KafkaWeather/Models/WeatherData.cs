using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaWeather.Models
{
    internal class WeatherData
    {
        public double TempCurrent { get; set; }

        public double TempMax { get; set; }

        public double TempMin { get; set; }

        public string Comment { get; set; }

        public DateTime TimeStamp { get; set; }

        public string City { get; set; }

        public int CityId { get; set; }
    }
}
