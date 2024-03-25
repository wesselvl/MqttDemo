using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttDemoControl
{
    internal class Device
    {
        private static int currentId;
        
        protected int Id { get; set; }
        public bool IsOn { get; set; } = false;
        public string SubscriptionTopic { get; set; }

        public Device(string subscriptionTopic)
        {
            SubscriptionTopic = subscriptionTopic;
            this.Id = GetNextId();
        }

        public void SwitchOn()
        {
            IsOn = true;
        }

        public void SwitchOff() 
        {  
            IsOn = false; 
        }

        public int DeviceId
        {
            get { return Id; }
        }

        protected int GetNextId()
        {
            return currentId++;
        }
    }
}
