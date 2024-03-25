using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttDemoControl
{
    internal class Light : Device
    {
        public int Intensity { get; set; } = 100;

        public Light(string subscriptionTopic) : base(subscriptionTopic) { }

        public Light(string subscriptionTopic, int intensity) : base(subscriptionTopic)
        {
            Intensity = intensity;
        }
    }
}
