/*using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMqMessagingAPI.Services
{
    public class RabbitMqService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost", // یا آدرس سرور RabbitMQ شما
                UserName = "mopix",
                Password = "72M991gh."
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // تعریف Exchange و Queue ها
            _channel.ExchangeDeclare("crm-exchange", ExchangeType.Direct);
            _channel.ExchangeDeclare("afkar-exchange", ExchangeType.Direct);

            _channel.QueueDeclare("sms_crm_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("email_crm_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("sms_afkar_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare("email_afkar_queue", durable: true, exclusive: false, autoDelete: false);

            // اتصال Queue ها به Exchange ها
            _channel.QueueBind("sms_crm_queue", "crm-exchange", "sms");
            _channel.QueueBind("email_crm_queue", "crm-exchange", "email");
            _channel.QueueBind("sms_afkar_queue", "afkar-exchange", "sms");
            _channel.QueueBind("email_afkar_queue", "afkar-exchange", "email");
        }

        public void SendMessage<T>(string exchange, string routingKey, T message)
        {
            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: null, body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}*//*

using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;

namespace NotificationService.Services
{
    public class RabbitMqService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqService(string hostName, string userName, string password)
        {
            // ساخت کانکشن به RabbitMQ
            var factory = new ConnectionFactory
            {
                HostName = hostName,
                UserName = userName,
                Password = password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        // متدهایی برای اعلام Exchange
        public void DeclareExchange(string exchangeName, string exchangeType = "direct", bool durable = true)
        {
            try
            {
                // اعلام Exchange با ویژگی‌های مشخص
                _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: durable, autoDelete: false);
                Console.WriteLine($"Exchange '{exchangeName}' declared successfully.");
            }
            catch (OperationInterruptedException ex)
            {
                // در صورتی که Exchange قبلاً با ویژگی‌های مختلفی ساخته شده باشد
                Console.WriteLine($"Exchange '{exchangeName}' exists with different properties. Deleting and re-creating...");

                // حذف Exchange موجود
                _channel.ExchangeDelete(exchange: exchangeName);
                Console.WriteLine($"Exchange '{exchangeName}' deleted.");

                // اعلام مجدد Exchange با ویژگی‌های جدید
                _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: durable, autoDelete: false);
                Console.WriteLine($"Exchange '{exchangeName}' re-declared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error declaring exchange '{exchangeName}': {ex.Message}");
            }
        }

        // متدهایی برای اعلام Queue
        public void DeclareQueue(string queueName)
        {
            try
            {
                // اعلام Queue با ویژگی‌های مناسب
                _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                Console.WriteLine($"Queue '{queueName}' declared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error declaring queue '{queueName}': {ex.Message}");
            }
        }

        // متدهایی برای Bind کردن Queue به Exchange
        public void BindQueue(string queueName, string exchangeName, string routingKey)
        {
            try
            {
                // Bind کردن Queue به Exchange با Routing Key
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
                Console.WriteLine($"Queue '{queueName}' bound to exchange '{exchangeName}' with routing key '{routingKey}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error binding queue '{queueName}' to exchange '{exchangeName}': {ex.Message}");
            }
        }

        // متدهایی برای ارسال پیام به Exchange
        public void Publish(string exchangeName, string routingKey, string message)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(message);
                // ارسال پیام به Exchange
                _channel.BasicPublish(exchange: exchangeName, routingKey: routingKey, basicProperties: null, body: body);
                Console.WriteLine($"Message published to exchange '{exchangeName}' with routing key '{routingKey}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing message to exchange '{exchangeName}': {ex.Message}");
            }
        }

        // متد برای پاک کردن منابع در پایان
        public void Dispose()
        {
            // بستن کانکشن و کانال
            _channel?.Close();
            _connection?.Close();
        }
    }
}
*/