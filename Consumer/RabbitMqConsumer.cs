using Microsoft.Extensions.Logging;
using ModelLayers.Models;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RepositoryLayers.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer
{
    public class RabbitMqConsumer
    {
        private readonly IConnectionFactory _factory;
        private readonly UserContext userContext;
        private readonly ILogger<RabbitMqConsumer> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _scopeFactory;

        public RabbitMqConsumer(IConnectionFactory factory, ILogger<RabbitMqConsumer> logger, UserContext userContext, IConfiguration configuration, IServiceScopeFactory scopeFactory)
        {
            _factory = factory;
            _logger = logger;
            this.userContext = userContext;
            _configuration = configuration;
            _scopeFactory = scopeFactory;
        }

        public async void StartConsuming(CancellationToken stoppingToken)
        {
            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "password_reset_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Received {0}", message);

                    var resetInfo = JsonSerializer.Deserialize<ResetInfo>(message);
                    if (resetInfo != null)
                    {
                        await ProcessPasswordReset(resetInfo, stoppingToken);
                    }
                };

                channel.BasicConsume(queue: "password_reset_queue",
                                     autoAck: true,
                                     consumer: consumer);

                _logger.LogInformation("Consumer started. Listening for messages...");

                // Keep the service running until the cancellation token is triggered
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        private async Task ProcessPasswordReset(ResetInfo resetInfo, CancellationToken cancellationToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var userContext = scope.ServiceProvider.GetRequiredService<UserContext>();
                var user = userContext.tblUser.FirstOrDefault(x => x.Email.Equals(resetInfo.Email));
                if (user != null)
                {
                    await SendPasswordResetEmail(user.Email, resetInfo.ResetToken);
                    _logger.LogInformation("Password reset email sent to {0}", user.Email);
                }
                else
                {
                    _logger.LogWarning("User with email {0} not found", resetInfo.Email);
                }
            }
        }

        private async Task SendPasswordResetEmail(string email, string resetToken)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpUser = _configuration["Smtp:Username"];
            var smtpPass = _configuration["Smtp:Password"];

            using var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = "Password Reset Request",
                Body = $"<p>You requested a password reset. Click the link below to reset your password:</p>" +
                       $"<p><a href='https://localhost:7197/api/User/resetPassword'>Reset Password</a></p>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Password reset email sent to {0}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send password reset email to {0}: {1}", email, ex.ToString());
            }
        }
    }
}
