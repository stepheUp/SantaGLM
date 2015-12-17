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
        static string[] gifts = new[] { "Doll", "Car", "Minecraft", "XBox", "Bike", "Ball", "Band", "Book"};
        static string[] cities = new[] { "Paris", "Bratislava", "Prague", "Berlin", "Moscow", "Madrid","Amsterdam" };
        static string[] emotions = new[] { "Anger", "Contempt", "Disgust", "Fear", "Happiness", "Neutral", "Sadness", "Surprise" };
        static string[] giftTypes = new[] { "car", "doll", "book", "bike", "xbox one", "play station 4"};

         


        static string[] box_states = new[] { "open", "close" };

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
                int randGiftID = rand.Next(0, gifts .Length);
                int randCityID = rand.Next(0, cities.Length);
                int randEmotionID = rand.Next(0, emotions .Length);
                int randBoxStateID = rand.Next(0, box_states.Length);
                int randGiftTypeID = rand.Next(0, giftTypes .Length);

                string giftrand = gifts [randGiftID];
                string cityrand = cities[randCityID];
                string emotionrand =  emotions[randEmotionID];
                string box_staterand = box_states [randBoxStateID];
                string giftTyperand = giftTypes [ randGiftTypeID ];


                var telemetryDataPoint = new
                {
                    deviceid = giftrand ,
                    location = cityrand ,
                    emotion = emotionrand ,
                    state = box_staterand,
                    giftType = giftTyperand  
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
