using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace SimulateAppDeviceToCloud
{
    class Program
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "SantaGLM-IoTHub.azure-devices.net";
        static string deviceKey = "oTXYY/HeTxfFY73OT26wqFMemuILqUqFLLUUQtTS+VU=";
        static string[] countries = new[] { "France", "Slovakia", "Czech Republic", "Germany", "Nederland", "Spain" };
        static string[] emotions = new[] { "happy", "superhappy", "neutral","cry"};
        static string[] box_statuses = new[] { "open", "close" };

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("santasFirstDevice", deviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }

        private static async void SendDeviceToCloudMessagesAsync()
        {

            Random rand = new Random();

            while (true)
            {
                int randCountryID = rand.Next(0, countries.Length);
                int randEmotionID = rand.Next(0, emotions .Length);
                int randBoxStatusID = rand.Next(0, box_statuses.Length);

                string countryrand = countries[randCountryID];
                string emotionrand =  countries[randEmotionID];
                string box_statusrand = countries[randBoxStatusID];


                var telemetryDataPoint = new
                {
                    deviceId = "santasFirstDevice",
                    location = countryrand ,
                    emotion = emotionrand ,
                    boxstatus = box_statusrand  
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(1000);
            }
        }
    }
}
