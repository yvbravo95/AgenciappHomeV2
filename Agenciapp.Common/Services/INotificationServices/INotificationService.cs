using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agenciapp.Common.Services.INotificationServices.Models;
using Agenciapp.Domain.Models.Notification;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Agenciapp.Common.Services.INotificationServices
{
    public interface INotificationService
    {
        Task<Result<string>> sendSms(String messaje, String to);
        Task<Result<string>> sendEmail(EmailAddress from, EmailAddress to, string title, string msg, IEnumerable<Attachment> attachments, bool isHtmlBody);
        Task<Result<Notification>> Create(CreateNotificationModel model);
        Task<Result<List<Notification>>> GetNewNotification();
        Task<Result<List<Notification>>> GetAllNotification();
        Task<Result<List<Notification>>> ReadNotification(List<Guid> ids);
        Task<Result<string>> SendMessageWhatsapp(string message, string to);
    }

    public class NotificationService: INotificationService
    {
        private readonly TwilioSetting _twilioSetting;
        private readonly SendgridSetting _sendgridSetting;
        private readonly databaseContext _context;
        private readonly IUserResolverService _user;
        public NotificationService(IOptions<TwilioSetting> twilioSetting, IOptions<SendgridSetting> sendgridSetting, databaseContext databaseContext, IUserResolverService user)
        {
            _twilioSetting = twilioSetting.Value;
            _sendgridSetting = sendgridSetting.Value;
            _context = databaseContext;
            _user = user;
        }

        public async Task<Result<Notification>> Create(CreateNotificationModel model)
        {
            try
            {
                var notification = new Notification(model.Title, model.Description, model.Status, model.Type, model.Employee);
                _context.Attach(notification);
                await _context.SaveChangesAsync();
                return Result.Success(notification);
            }
            catch(Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<Notification>("No se ha podido crear la notificación");
            }
        }

        public async Task<Result<List<Notification>>> GetNewNotification()
        {
            try
            {
                var employee = _user.GetUser();
                var notifications = await _context.Notifications.Where(x => x.Employee == employee && x.Status == NotificationStatus.UnRead).ToListAsync();
                return Result.Success(notifications);
            }
            catch(Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<List<Notification>>("No se han podido obtener las notificaciones");
            }
        }

        public async Task<Result<List<Notification>>> GetAllNotification()
        {
            try
            {
                var employee = _user.GetUser();

                var notifications = await _context.Notifications.Where(x => x.Employee == employee).ToListAsync();
                return Result.Success(notifications);
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<List<Notification>>("No se han podido obtener las notificaciones");
            }
        }

        public async Task<Result<List<Notification>>> ReadNotification(List<Guid> ids)
        {
            try
            {
                var employee = _user.GetUser();

                var notifications = _context.Notifications.Where(x => x.Employee == employee && ids.Contains(x.Id));
                foreach (var item in notifications)
                {
                    item.ChangeStatus(NotificationStatus.Read);
                    _context.Update(item);
                }
                await _context.SaveChangesAsync();

                return await GetNewNotification();
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<List<Notification>>("No se han podido leer las notificaciones");
            }
        }

        public async Task<Result<string>> sendEmail( EmailAddress from, EmailAddress to, string title, string msg, IEnumerable<Attachment> attachments, bool isHtmlBody)
        {
            try
            {
                if(from == null)
                {
                    from = new EmailAddress
                    {
                        Email = "do_not_reply@agenciapp.com",
                        Name = "From AgenciaApp"
                    };
                }
                else if(from.Email == null)
                {
                    from.Email = "do_not_reply@agenciapp.com";
                    from.Name = "From AgenciaApp";
                }
                var client = new SendGridClient(_sendgridSetting.SendGridClient);

                var msgs = new SendGridMessage()
                {
                    From = from,
                    Subject = title,
                    PlainTextContent = isHtmlBody == false? msg: "",
                    HtmlContent = isHtmlBody? msg: ""
                };
                msgs.AddTo(to);

                if(attachments != null && attachments.Any())
                    msgs.AddAttachments(attachments);

                var responses = await client.SendEmailAsync(msgs);
                var responseBody = responses.Body.ReadAsStringAsync().Result;
                Serilog.Log.Information($"Send Email {responses.StatusCode.ToString()}");
                if (responses.StatusCode != System.Net.HttpStatusCode.Accepted)
                    return Result.Failure<string>($"No se ha podido enviar el email. Status Code {responses.StatusCode.ToString()}");
                return Result.Success(responses.StatusCode.ToString());
            }
            catch (Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<string>("No se ha podido enviar el mensaje");
            }

        }

        public async Task<Result<string>> sendSms(string messaje, string to)
        {
            try
            {
                TwilioClient.Init(_twilioSetting.AccountSid, _twilioSetting.AuthToken);
                var message = await MessageResource.CreateAsync(
                    body: messaje,
                    from: new Twilio.Types.PhoneNumber(_twilioSetting.PhoneFrom),
                    to: new Twilio.Types.PhoneNumber(formatPhone(to))
                    );
                var response = message.Sid;
                return Result.Success("El mensaje ha sido enviado");
            }
            catch(Exception e)
            {
                Serilog.Log.Fatal(e, "Server Error");
                return Result.Failure<string>("No se ha podido enviar el mensaje");
            }
        }

        public async Task<Result<string>> SendMessageWhatsapp(string message, string to)
        {
            var user = _user.GetUser();
            var whatsapp = new WhatsappApi("instance76084", "mlec03tl34buc4u6");    
            whatsapp.SendWhatsappMessage(to, message);
            return Result.Success("El mensaje ha sido enviado");
        }

        private string formatPhone(string phone)
        {
            string result = "";
            for (int i = 0; i < phone.Length; i++)
            {
                char x = phone.ElementAt(i);
                if ((int)x >= 48 && (int)x <= 57)
                {
                    result = result + x;
                }
            }
            if (result.Length > 10)
            {
                result = result.Substring(result.Length - 10);
            }
            return result;
        }
    }
}
