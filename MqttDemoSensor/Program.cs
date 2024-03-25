using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

class Program
{
    static async Task Main(string[] args)
    {
        int NUM_MODES = 2;
        #region MQTT connection
        string broker = "broker.emqx.io";
        int port = 1883;
        string clientId = Guid.NewGuid().ToString();
        string username = "darwinDemoSensor";
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
            int mode = NUM_MODES;
            Random rng = new Random();

            Console.WriteLine("Successfully connected to MQTT broker.");

            // Mode selection prompt
            while (mode >= NUM_MODES)
            {
                Console.WriteLine("Select mode (0 = automatic, 1 = manual)");
                mode = Convert.ToInt32(Console.ReadLine());
            }

            // Sensor location prompt
            string? loc = null;

            while (loc == null)
            {
                Console.WriteLine("Enter sensor location:");
                loc = Console.ReadLine();
            }
            
            switch (mode)
            {
                case 0:
                    await Automatic(loc);
                    break;
                case 1:
                    await Manual(loc);
                    break;     
            }

            // Automatic mode logic
            async Task Automatic(string sensorLocation)
            {
                var sensorOnMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(baseTopic + "/" + sensorLocation.ToLower())
                        .WithPayload("true")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();
                var sensorOffMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(baseTopic + "/" + sensorLocation.ToLower())
                        .WithPayload("false")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag()
                        .Build();

                while (true)
                {
                    await mqttClient.PublishAsync(sensorOnMessage);
                    Console.WriteLine($"Motion detected on {sensorLocation} sensor.");
                    await Task.Delay(rng.Next(3000, 8000));
                    await mqttClient.PublishAsync(sensorOffMessage);
                    Console.WriteLine($"Motion has stopped on {sensorLocation} sensor.");
                    await Task.Delay(rng.Next(3000, 8000));
                }
            }

            // Manual mode logic
            async Task Manual(string sensorLocation)
            {
                var sensorOnMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(baseTopic + "/" + sensorLocation.ToLower())
                        .WithPayload("true")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .Build();
                var sensorOffMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(baseTopic + "/" + sensorLocation.ToLower())
                        .WithPayload("false")
                        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                        .WithRetainFlag()
                        .Build();

                while (true)
                {
                    Console.WriteLine("Turn sensor off (0) or on (1).");

                    int onOffSwitch = Convert.ToInt32(Console.ReadLine());

                    if (onOffSwitch == 0)
                    {
                        await mqttClient.PublishAsync(sensorOffMessage);
                        Console.WriteLine($"{sensorLocation} sensor turned off.");
                    }

                    if (onOffSwitch == 1)
                    {
                        await mqttClient.PublishAsync(sensorOnMessage);
                        Console.WriteLine($"{sensorLocation} sensor turned on.");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine($"Failed to connect to MQTT broker: {connectResult.ResultCode}");
        }
    }
}