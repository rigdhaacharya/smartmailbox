// ------------------------------------------------------------------------------
// <copyright file="HomeController.cs">
//  This controller receives messages from iot device and forwards it to the IoT hub
// </copyright>
// ------------------------------------------------------------------------------

namespace TrainMailEdge.Controllers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;

    /// <summary>
    /// This controller receives the ID from the IoT device and sends it to the IoT hub for the function to process
    /// </summary>
    public class HomeController : Controller
    {
        private static DeviceClient s_deviceClient;
        
        private static readonly string s_connectionString = "insert_iothub_connection_here";

        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync(string id)
        {
            // Create JSON message
            var datapoint = new
            {
                id
            };
            var messageString = JsonConvert.SerializeObject(datapoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            // Send the telemetry message
            await s_deviceClient.SendEventAsync(message);
            await Task.Delay(1000);
        }

        // GET: Home
        public async Task<ActionResult> Index(string id)
        {
            await RegisterRfid(id);
            return Content("OK");
        }

        private async Task RegisterRfid(string id)
        {
            // Connect to the IoT hub using the MQTT protocol
            s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
            await SendDeviceToCloudMessagesAsync(id);
        }
    }
}
