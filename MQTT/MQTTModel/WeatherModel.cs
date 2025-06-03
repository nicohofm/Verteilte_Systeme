using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace MQTT.MQTTModel
{
    internal class WeatherModel
    {
        //public string topic { get; set; }
        //public byte[] payload { get; set; }
        //public DateTime timestamp { get; set; }
        public float tempCurrent { get; set; }
        public float tempMax { get; set; }
        public float tempMin { get; set; }
        public string comment { get; set; }
        public DateTime timeStamp { get; set; }
        public string city { get; set; }
        public int cityId { get; set; }
    }
}
