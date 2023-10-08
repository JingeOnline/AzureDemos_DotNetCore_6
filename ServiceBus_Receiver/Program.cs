using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using ServiceBus_SharedLibrary;
using System.Text.Json;

namespace ServiceBus_Receiver
{
    public class Program
    {
        static ServiceBusClient client;
        static ServiceBusProcessor processor;
        static string queueName = "jingetestqueue";

        static async Task Main(string[] args)
        {
            string connectString = getJsonConfig("AzureServiceBusConnectionString");
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets  //使用443端口,如果不指定,默认使用5671和5672 
            };
            client = new ServiceBusClient(connectString, clientOptions);
            processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

            try
            {
                processor.ProcessMessageAsync += MessageHandler;
                processor.ProcessErrorAsync += ErrorHandler;
                Console.WriteLine("Message received: (Press any key to exit)");
                //开始接收消息
                await processor.StartProcessingAsync();
                Console.ReadKey();
                //结束接收消息
                await processor.StopProcessingAsync();
            }
            finally
            {
                await processor.DisposeAsync();
                await client.DisposeAsync();
            }
        }

        //处理接收到的消息
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            MessageModel? messageModel = JsonSerializer.Deserialize<MessageModel>(body);
            Console.WriteLine();
            Console.WriteLine($"Sender: {messageModel?.Sender}");
            Console.WriteLine($"Time: {messageModel?.SendTime}");
            Console.WriteLine($"Content: {messageModel?.Content}");
            Console.WriteLine();

            //需要显式的调用消息接收完成的方法，之后该消息会从queue中删除
            await args.CompleteMessageAsync(args.Message);
        }

        //处理接收消息过程中的异常
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

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