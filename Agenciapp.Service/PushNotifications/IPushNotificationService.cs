using Agenciapp.Service.PushNotifications.Models;
using Microsoft.Extensions.Logging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Messaging;
using Newtonsoft.Json;

namespace Agenciapp.Service.PushNotifications
{
    public interface IPushNotificationService
    {
        Task<SendPushNotificationResult> SendPushNotification(SendPushNotificationRequest request, string fileName);
    }


    public class PushNotificationService : IPushNotificationService
    {
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(ILogger<PushNotificationService> logger)
        {
            _logger = logger;
        }

        private int _maxTokenAmount = 100;

        public async Task<SendPushNotificationResult> SendPushNotification(SendPushNotificationRequest request, string fileName)
        {
            var fcmTokens = request.Tokens.Distinct().ToList();

            var defaultApp = FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = await GoogleCredential.FromFileAsync(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, fileName),
                    CancellationToken.None)
            });
            var messaging = FirebaseMessaging.GetMessaging(defaultApp);
            var requestAmount = fcmTokens.Count % _maxTokenAmount == 0
                ? fcmTokens.Count / _maxTokenAmount
                : (fcmTokens.Count / _maxTokenAmount) + 1;
            var responses = new List<BatchResponse>();

            for (var i = 0; i < requestAmount; i++)
            {
                var tokens = fcmTokens
                    .Skip(i * _maxTokenAmount)
                    .Take(i == requestAmount - 1 ? (fcmTokens.Count - i * _maxTokenAmount) : _maxTokenAmount)
                    .ToList();
                var message = new MulticastMessage()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = request.Title,
                        Body = request.Body,
                        ImageUrl = request.ImageUrl
                    },
                    Tokens = tokens
                };


                try
                {
                    var response = await messaging.SendMulticastAsync(message);
                    responses.Add(response);
                }
                catch (FirebaseMessagingException e)
                {
                    _logger.LogError(JsonConvert.SerializeObject(e));
                }
            }

            return new SendPushNotificationResult
            {
                Failed = responses.Sum(x => x.FailureCount),
                Total = fcmTokens.Count,
                Succeed = responses.Sum(x => x.SuccessCount),
            };
        }
    }
}
