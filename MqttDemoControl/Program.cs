using System.Text;
using MqttDemoControl;
using MQTTnet;
using MQTTnet.Client;

class Program
{
    static async Task Main(string[] args)
    {   
        // Set up MQTT connection
        #region MQTT connection
        string broker = "broker.emqx.io";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string username = "darwinDemoControl";
        string password = "hunter2";
        string baseTopic = "darwinDemo";

        var factory = new MqttFactory();

        var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();

        var connectResult = await mqttClient.ConnectAsync(options);
        #endregion

        if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
        {
            // Set up list of lights and the topics they "listen" on
            #region Light setup
            List<Light> lights = new List<Light>();
            Light lightNorth1 = new Light(baseTopic + "/north", 70);
            Light lightNorth2 = new Light(baseTopic + "/north", 50);
            Light lightSouth = new Light(baseTopic + "/south");
            lights.Add(lightNorth1);
            lights.Add(lightNorth2);
            lights.Add(lightSouth);
            #endregion


            Console.WriteLine("Successfully connected to MQTT broker.");

            // Subscribe to all topics related to demo
            await mqttClient.SubscribeAsync(baseTopic + "/#");

            Console.WriteLine($"Subscribed to topic {baseTopic}/#.");

            // Receive MQTT messages and manipulate lights accordingly
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                string msg = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                Console.WriteLine($"Received MQTT message on topic {e.ApplicationMessage.Topic}: {msg}.");

                bool motionDetected = bool.Parse(msg);
                
                foreach (Light l in lights)
                {
                    if (l.SubscriptionTopic == e.ApplicationMessage.Topic)
                    {
                        if (motionDetected)
                        {
                            l.SwitchOn();
                        }
                        else
                        {
                            l.SwitchOff();
                        }
                    }
                }
                return Task.CompletedTask;
            };

            // Status display loop
            while (true)
            {
                Console.WriteLine("--- Status: ---");
                foreach (Light l in lights)
                {
                    if (l.IsOn)
                    {
                        Console.WriteLine($"Light with ID {l.DeviceId} is on with intensity {l.Intensity}.");
                    }
                    else
                    {
                        Console.WriteLine($"Light with ID {l.DeviceId} is off.");
                    }
                }

                await Task.Delay(2000);
            }
        }
        else
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        }
    }
}