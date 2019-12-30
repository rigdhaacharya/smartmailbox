// ------------------------------------------------------------------------------
// <copyright file="Function1.cs">
//   This is the function that processes messages from the IoT device. It will
//   call the machine learning module and determine if the given mail is spam or not.
//   I used the IoT hub trigger function template from Visual Studio which sets up the broilerplate code
//   Twilio example from https://www.twilio.com/docs/libraries/csharp-dotnet
// </copyright>
// ------------------------------------------------------------------------------

using IoTHubTrigger = Microsoft.Azure.WebJobs.ServiceBus.EventHubTriggerAttribute;

namespace FunctionApp1
{
    using System;
    using System.Data.SqlClient;
    using System.Net.Http;
    using System.Text;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Twilio;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;

    public static class Function1
    {
        private static readonly string cString = "insert_iothub_connection_string_here";

        /// <summary>
        /// This function runs every time a message is received from IoT hub
        /// </summary>
        /// <param name="message">Message from IoT Hub</param>
        /// <param name="log">Logger</param>
        [FunctionName("Function1")]
        public static void Run(
            [IoTHubTrigger("messages/events", Connection = "IoTHubConnectionString")]
            EventData message,
            TraceWriter log)
        {
            try
            {
                log.Info(
                    $"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.GetBytes())}");
                var messageString = Encoding.UTF8.GetString(message.GetBytes());
                log.Info(messageString);
                var messageJson = (JObject) JsonConvert.DeserializeObject(messageString);
                log.Info("got message json");
                if (!string.IsNullOrEmpty((string) messageJson["id"]))
                {
                    log.Info("getting info from db");
                    //get the sender and receiver info from db to send to ML module
                    var mailInfo = GetMailInfo((string) messageJson["id"], log);
                    var sender = mailInfo.fromuser;
                    var receiver = mailInfo.touser;
                    var requestJson = new
                    {
                        fromuser = sender,
                        touser = receiver
                    };
                    log.Info("Sending request for classification for " +
                             JsonConvert.SerializeObject(requestJson, Formatting.Indented));
                    using (var client = new HttpClient())
                    {
                        //get the classification score
                        var response = client.PostAsync(
                            "http://13.64.31.235:80/score",
                            new StringContent(JsonConvert.SerializeObject(requestJson), Encoding.UTF8,
                                "application/json")).Result;
                        var respondeString = response.Content.ReadAsStringAsync().Result;
                        var json = (JArray) JsonConvert.DeserializeObject(respondeString);

                        //parse the response JSON from the ML module
                        foreach (JValue jObject in json)
                        {
                            var jsonRep = (JObject) JsonConvert.DeserializeObject((string) jObject.Value);
                            var isSpam = (string) jsonRep["anomaly"];
                            // Find your Account Sid and Token at twilio.com/console
                            const string accountSid = "insert_twilio_account_sid";
                            const string authToken = "insert_twilio_auth_token";

                            TwilioClient.Init(accountSid, authToken);

                             MessageResource.Create(
                                body:
                                $"New mail received from {mailInfo.senderName}. Spam={isSpam}. Update score at https://bit.ly/2J5ahLb",
                                from: new PhoneNumber("twilio_from_number"),
                                to: new PhoneNumber("twilio_to_number")
                            );
                        }
                    }
                }
                else
                {
                    log.Info("message id is null");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Gets information about a mail item given it's tag
        /// </summary>
        /// <param name="tag">TagId</param>
        /// <param name="log">Logger</param>
        /// <returns></returns>
        private static MailInfo GetMailInfo(string tag, TraceWriter log)
        {
            var mailInfo = new MailInfo();
            using (SqlConnection c = new SqlConnection(cString))
            {
                c.Open();
                using (SqlCommand cmd =
                    new SqlCommand(
                        $"SELECT username, Sender,Receiver FROM tagInfo join usertable on Sender=userid where tagId='{tag}'",
                        c))
                {
                    log.Info(cmd.ToString());
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            mailInfo.senderName = rdr.GetString(0);
                            mailInfo.fromuser = rdr.GetInt32(1);
                            mailInfo.touser = rdr.GetInt32(2);
                            return mailInfo;
                        }
                    }
                }
            }

            return mailInfo;
        }
    }

    class MailInfo
    {
        public int fromuser { get; set; }
        public int touser { get; set; }
        public string senderName { get; set; }
    }
    
}
