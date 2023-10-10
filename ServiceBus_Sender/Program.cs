using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using ServiceBus_SharedLibrary;
using System.Text;
using System.Text.Json;

namespace ServiceBus_Sender
{
    public class Program
    {
        static ServiceBusClient client;
        static ServiceBusSender sender;
        static string queueName = "jingetestqueue";

        static async Task Main(string[] args)
        {
            string connectString = getJsonConfig("AzureServiceBusConnectionString");
            var clientOptions = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets,  //使用443端口,如果不指定,默认使用5671和5672 
                Identifier="jinge"   //为Identifier赋值并不起作用，估计basic套餐不支持。
            };
            client = new ServiceBusClient(connectString,clientOptions);
            sender = client.CreateSender(queueName);

            try
            {
                while (true)
                {
                    MessageModel messageModel = new MessageModel();
                    Console.WriteLine("Please input username:");
                    messageModel.Sender = Console.ReadLine();
                    Console.WriteLine("Please input message");
                    messageModel.Content = Console.ReadLine();
                    messageModel.SendTime = DateTime.Now;
                    await SendMessageAsync(messageModel);
                    Console.WriteLine("Send Succeed. Continue? Y/N");
                    if (String.Compare(Console.ReadLine(), "N", true) == 0)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
            
        }

        public static async Task SendMessageAsync<T>(T serviceBusMessage)
        {
            string jsonText = JsonSerializer.Serialize(serviceBusMessage);
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonText));
            await sender.SendMessageAsync(message);

        }

        #region 下面的代码使用了Microsoft.Azure.ServiceBus包，该包已经弃用
        //static QueueClient queueclient;
        //static string queueName = "jingetestqueue";

        //static async Task Main(string[] args)
        //{
        //    string connectString = getJsonConfig("AzureServiceBusConnectionString");
        //    queueclient = new QueueClient(connectString, queueName);

        //    while(true)
        //    {
        //        MessageModel messageModel = new MessageModel();
        //        Console.WriteLine("Please input username:");
        //        messageModel.Sender = Console.ReadLine();
        //        Console.WriteLine("Please input message");
        //        messageModel.Content = Console.ReadLine();
        //        messageModel.SendTime = DateTime.Now;
        //        await SendMessageAsync(messageModel);
        //        Console.WriteLine("Send Succeed. Continue? Y/N");
        //        if(String.Compare(Console.ReadLine(),"N",true)==0)
        //        {
        //            break;
        //        }
        //    }
        //    await queueclient.CloseAsync();
        //}

        //public static async Task SendMessageAsync<T>(T serviceBusMessage)
        //{
        //    string jsonText=JsonSerializer.Serialize(serviceBusMessage);
        //    Message message = new Message(Encoding.UTF8.GetBytes(jsonText));
        //    await queueclient.SendAsync(message);

        //}
        #endregion

        //获取本地文件夹中储存的密钥，防止密钥泄露到GitHub上。
        public static string getJsonConfig(string key)
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(userFolder, "OneDrive\\脚本代码\\AccountSecrets.json");
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(filePath);
            IConfiguration config = builder.Build();
            var section = config.GetSection(key);
            return section.Value;
        }
    }
}