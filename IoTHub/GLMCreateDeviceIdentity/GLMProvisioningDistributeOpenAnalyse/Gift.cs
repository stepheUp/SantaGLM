using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;

namespace GLMProvisioningDistributeOpenAnalyse
{
    public class Gift
    {
        public string DeviceID { get; set; }
        public string DeviceKey { get; set; }
        public string Location { get; set; }
        public string Emotion { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public string Rating { get; set; }

        public async Task RemoveAsync()
        {
            RegistryManager registryManager;
            string connectionString = "HostName=SantaGLM-IoTHub.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=+KG5ihUM8LuE3MStlQF1CCCzO7p05YwvkVPYTwiMyzo=";

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

               await registryManager.RemoveDeviceAsync(new Device(DeviceID));

            Console.WriteLine("Removed device {0}",  DeviceID);

        }

        public async Task RegisterAsync()
        {
            RegistryManager registryManager;
            string connectionString = "HostName=SantaGLM-IoTHub.azure-devices.net;SharedAccessKeyName=registryReadWrite;SharedAccessKey=+KG5ihUM8LuE3MStlQF1CCCzO7p05YwvkVPYTwiMyzo=";

            registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(DeviceID));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(DeviceID);
            }

            DeviceKey = device.Authentication.SymmetricKey.PrimaryKey;
            // gift.DeviceClientHub = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(gift.DeviceID, gift.DeviceKey));


            Console.WriteLine("Generated device key: {0} for device {1}", DeviceKey, DeviceID);

        }

        public async Task SendGiftToCloudMessagesAsync()
        {
            string iotHubUri = "SantaGLM-IoTHub.azure-devices.net";

            DeviceClient deviceClient;
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("santasFirstDevice", "oTXYY/HeTxfFY73OT26wqFMemuILqUqFLLUUQtTS+VU="));

            var telemetryDataPoint = new
            {
                deviceId = DeviceID,
                location = Location,
                emotion = Emotion,
                state = State,
                giftType = Type,
                rating = Rating
            };
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));


            await deviceClient.SendEventAsync(message);
            Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

        }
    }
}
