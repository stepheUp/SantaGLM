using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;


namespace GLMProvisioningDistributeOpenAnalyse
{

    class Program
    {
        static RegistryManager registryManager;
        static string connectionString = "HostName=SantaGLM-IoTHub.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=+KG5ihUM8LuE3MStlQF1CCCzO7p05YwvkVPYTwiMyzo=";
        static List<Gift> Gifts = new List<Gift>();

        static string[] gifts = new string[] { "Gift 01", "Gift 02", "Gift 03", "Gift 04", "Gift 05" };
        static string[] locations = new string[] { "Paris", "Prague", "Bratislava", "Moscow", "Amsterdam" };
        static string[] emotions = new string[] { "Sadness", "Anger", "Disgust", "Contempt", "Fear", "Neutral", "Surprise", "Happiness" };
        static string[] states = new string[] { "close", "open" };
        static string[] giftTypes = new string[] { "car", "doll", "book", "bike", "xbox one", "play station 4" };

        static string iotHubUri = "SantaGLM-IoTHub.azure-devices.net";

        static void Main(string[] args)
        {
            int numberOfGift = 200;
            InitGifts(numberOfGift);

            Console.ReadLine();

            //registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            //RegisterGiftsAsync().Wait();
            //Console.ReadLine();

            ProvisionningGiftsAsync().Wait();
            Console.ReadLine();

            DistributeGiftsAsync().Wait();
            Console.ReadLine();

            OpenGiftsAsync().Wait();
            Console.ReadLine();

            AnalyseGiftsAsync().Wait();
            Console.ReadLine();

        }

        private static void InitGifts(int numberOfGift)
        {
            for (int i = 0; i < numberOfGift; i++)
            {
                Gifts.Add(new Gift() { DeviceID = "Gift_" + i });
            }
        }

        private async static Task RemoveAsync()
        {
            List<Task> tasks = new List<Task>();
            int throttlingLimit = 20; int j = 0;

            for (int i = 0; i < 500; i++)
            {
                if (throttlingLimit == j)
                {
                    await Task.WhenAll(tasks);
                    Console.WriteLine("sleep -_-\n");
                    //Thread.Sleep(5000);
                    j = 0;
                    tasks = new List<Task>();
                }
                Gift gift = new Gift() { DeviceID = "Gift_" + i };
                tasks.Add(gift.RemoveAsync());
                j++;
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Gift registered\n");
        }

        private async static Task RegisterGiftsAsync()
        {
            List<Task> tasks = new List<Task>();
            int throttlingLimit = 20; int j = 0;

            for (int i = 0; i < Gifts.Count; i++)
            {
                if (throttlingLimit == j)
                {
                    await Task.WhenAll(tasks);
                    Console.WriteLine("sleep -_-\n");
                    Thread.Sleep(5000);
                    j = 0;
                    tasks = new List<Task>();
                }
                tasks.Add(Gifts[i].RegisterAsync());
                j++;
            }

            await Task.WhenAll(tasks);

            Console.WriteLine("Gift registered\n");
        }


        private static async Task ProvisionningGiftsAsync()
        {
            Random rand = new Random();
            List<Task> tasks = new List<Task>();

            foreach (var gift in Gifts)
            {
                gift.State = states[0];
                gift.Type = giftTypes[rand.Next(giftTypes.Length)];
                tasks.Add(gift.SendGiftToCloudMessagesAsync());
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Provisionning done!\n");
        }

        private static async Task DistributeGiftsAsync()
        {
            Random rand = new Random();
            List<Task> tasks = new List<Task>();

            foreach (var gift in Gifts)
            {
                gift.Location = locations[rand.Next(locations.Length)];
                tasks.Add(gift.SendGiftToCloudMessagesAsync());
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Distribution done!\n");
        }

        private static async Task OpenGiftsAsync()
        {
            Random rand = new Random();
            List<Task> tasks = new List<Task>();

            foreach (var gift in Gifts)
            {
                gift.State = states[1];
                tasks.Add(gift.SendGiftToCloudMessagesAsync());
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Opening done!\n");
        }

        private static async Task AnalyseGiftsAsync()
        {
            Random rand = new Random();
            List<Task> tasks = new List<Task>();

            foreach (var gift in Gifts)
            {
                int index = rand.Next(emotions.Length);
                gift.Emotion = emotions[index];
                gift.Rating = index.ToString();
                tasks.Add(gift.SendGiftToCloudMessagesAsync());
            }

            await Task.WhenAll(tasks);
            Console.WriteLine("Analyse done!\n");
        }


    }
}
