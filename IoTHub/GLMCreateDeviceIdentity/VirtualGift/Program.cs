using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;

namespace VirtualGift
{
    class Program
    {

        static DeviceClient deviceClient;
        static string iotHubUri = "SantaGLM-IoTHub.azure-devices.net";
        static string deviceKey = "oTXYY/HeTxfFY73OT26wqFMemuILqUqFLLUUQtTS+VU=";

        static void Main(string[] args)
        {
            Console.WriteLine("Simulated device\n");
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("santasFirstDevice", deviceKey));

            SendDeviceToCloudMessagesAsync();
            Console.ReadLine();
        }


              private static async void SendDeviceToCloudMessagesAsync()
        {
            string[] gifts = new string[] { "Gift 01", "Gift 02", "Gift 03", "Gift 04", "Gift 05" };
            string[] locations = new string[] { "Paris", "Prague", "Bratislava", "Moscow", "Amsterdam" };
            string[] emotions = new string[] { "Anger", "Contempt", "Disgust", "Fear", "Happiness", "Neutral", "Sadness", "Surprise" };
            string[] states = new string[] { "close", "open" };
            string[] giftTypes = new string[] { "car", "doll", "book", "bike", "xbox one", "play station 4" };

            Random rand = new Random();

            while (true)
            {
                var telemetryDataPoint = new
                {
                    deviceId = gifts[rand.Next(gifts.Length)],
                    location = locations[rand.Next(locations.Length)],
                    emotion = emotions[rand.Next(emotions.Length)],
                    state = states[rand.Next(states.Length)],
                    giftType = giftTypes[rand.Next(giftTypes.Length)]
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                Thread.Sleep(100);
            }
        }
    }
}
