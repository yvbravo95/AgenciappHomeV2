using Agenciapp.Common.Services;
using Agenciapp.Common.Services.INotificationServices;
using Agenciapp.Domain.Models.BuildEmail;
using Agenciapp.Service.IBuildEmailServices.Models;
using AgenciappHome.Models;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Log = Serilog.Log;

namespace Agenciapp.Service.IBuildEmailServices
{
    public interface IBuildEmailService
    {
        Task<Result<EmailTemplate>> CreateEmailTemplateAsync(CreateEmailTemplateModel model);
        Task<Result<EmailTemplate>> UpdateEmailTemplate(UpdateEmailTemplateModel model);
        Task<Result<string>> DeleteEmailTemplate(Guid Id);
        Task<Result<List<EmailTemplate>>> ListEmailTemplate();
        Task<Result<EmailBody>> CreateEmailBodyAsync(CreateEmailBodyModel model);
        Task<Result<EmailBody>> UpdateEmailBody(UpdateEmailBodyModel model);
        Task<Result<string>> DeleteEmailBody(Guid Id);
        Task<Result<List<EmailBody>>> ListEmailBody(Guid AgencyId);
        Task<Result<string>> SendBuildEmail(Guid emailBodyId, Guid clientId, Dictionary<string, string> values, List<Attachment> attachments = null);
    }

    public class BuildEmailService : IBuildEmailService
    {
        private readonly databaseContext _context;
        private readonly IUserResolverService _user;
        private readonly IHostingEnvironment _env;
        private readonly INotificationService _notification;
        public BuildEmailService(databaseContext context, IHostingEnvironment env, IUserResolverService user, INotificationService notification)
        {
            _context = context;
            _env = env;
            _user = user;
            _notification = notification;
        }

        public async Task<Result<EmailBody>> CreateEmailBodyAsync(CreateEmailBodyModel model)
        {
            try
            {
                var template = await _context.EmailTemplates.FindAsync(model.IdEmailTemplate);
                if (template == null)
                {
                    return Result.Failure<EmailBody>("La plantilla no existe.");
                }
                var body = new EmailBody
                {
                    Body = model.Body,
                    EmailTemplate = template,
                    Name = model.Name,
                };
                _context.EmailBodies.Attach(body);
                string sWebRootFolder = _env.WebRootPath;
                var date = DateTime.Now.ToString("yMMddHHmmssff");
                var count = 0;
                foreach (var item in model.Files)
                {
                    count++;
                    var auxName = item.FileName;
                    var arrName = auxName.Split('.');
                    string filename = date + count + '.' + arrName[1];
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "Upload" + Path.DirectorySeparatorChar + "EmailAttachment";
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    filePath = Path.Combine(filePath, filename);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await item.CopyToAsync(fileStream);
                    }
                    var attach = new EmailAttached
                    {
                        EmailBody = body,
                        OriginalName = item.FileName,
                        Name = filename
                    };
                    _context.EmailAttacheds.Attach(attach);
                }
                await _context.SaveChangesAsync();
                return Result.Success(body);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<EmailBody>("No se ha podido crear el email.");
            }
        }

        public async Task<Result<EmailTemplate>> CreateEmailTemplateAsync(CreateEmailTemplateModel model)
        {
            try
            {
                var user = _user.GetUser();
                if (user == null)
                {
                    return Result.Failure<EmailTemplate>("Debe estar autenticado");
                }
                var template = new EmailTemplate
                {
                    AgencyId = user.AgencyId,
                    Name = model.Name,
                    Template = model.Template
                };
                _context.EmailTemplates.Attach(template);
                await _context.SaveChangesAsync();

                return Result.Success(template);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<EmailTemplate>("No se ha podido crear la plantilla de email");
            }
        }

        public Task<Result<string>> DeleteEmailBody(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<string>> DeleteEmailTemplate(Guid Id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<EmailBody>>> ListEmailBody(Guid AgencyId)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<List<EmailTemplate>>> ListEmailTemplate()
        {
            try
            {
                var user = _user.GetUser();
                if (user == null)
                {
                    return Result.Failure<List<EmailTemplate>>("Debe estar autenticado.");
                }
                return Result.Success(await _context.EmailTemplates.Where(x => x.AgencyId == user.AgencyId).ToListAsync());
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<List<EmailTemplate>>("No se han podido obtener las plantillas de emails");

            }
        }

        public async Task<Result<string>> SendBuildEmail(Guid emailBodyId, Guid clientId, Dictionary<string, string> values, List<Attachment> attachments = null)
        {
            try
            {
                var emailBody = await _context.EmailBodies
                    .Include(x => x.EmailTemplate).ThenInclude(x => x.Agency).ThenInclude(x => x.EmailAddress)
                    .Include(x => x.EmailAttacheds)
                    .FirstOrDefaultAsync(x => x.Id == emailBodyId);
                if (emailBody == null)
                {
                    return Result.Failure<string>("No existe el email.");
                }
                var client = await _context.Client.FindAsync(clientId);
                if (client == null)
                {
                    return Result.Failure<string>("No existe el cliente.");
                }
                string msg = emailBody.EmailTemplate.Template;
                msg = msg.Replace("{{body}}", emailBody.Body);
                msg = msg.Replace("{{client_name}}", client.FullData);
                if (values != null)
                {
                    foreach (var item in values)
                    {
                        msg = msg.Replace("{{" + item.Key + "}}", item.Value);
                    }
                }
                if (attachments == null)
                    attachments = new List<Attachment>();
                string sWebRootFolder = _env.WebRootPath;
                foreach (var item in emailBody.EmailAttacheds)
                {
                    string filePath = sWebRootFolder + Path.DirectorySeparatorChar + "Upload" + Path.DirectorySeparatorChar + "EmailAttachment";
                    filePath = Path.Combine(filePath, item.Name);

                    Byte[] bytes = File.ReadAllBytes(filePath);
                    String file = Convert.ToBase64String(bytes);
                    var attach = new Attachment { Content = file, Filename = item.OriginalName };
                    attachments.Add(attach);
                }
                string emailClient = client.Email;
                if(emailBody.EmailTemplate?.AgencyId == Guid.Parse("4752B08A-7684-42B3-930D-FF86F496DF2F") && (emailClient.Contains("yahoo") || emailClient.Contains("hotmail"))){
                    emailClient = "info@districtcuba.com";
                }
                var response = await _notification.sendEmail(
                    new EmailAddress { Email = emailBody.EmailTemplate.Agency.EmailAddress?.Email, Name = emailBody.EmailTemplate.Agency.EmailAddress?.Name },
                    new EmailAddress { Email = emailClient, Name = client.Name },
                    emailBody.Name,
                    msg,
                    attachments.Any() ? attachments : null,
                    true
                    );
                if (response.IsFailure)
                {
                    return Result.Failure<string>(response.Error);
                }

                var registerEmail = new RegistrationSendEmail(client, emailBody, "");
                _context.RegistrationSendEmails.Attach(registerEmail);
                await _context.SaveChangesAsync();

                return Result.Success("El mensaje fué enviado");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Server Error");
                return Result.Failure<string>("No se ha podido enviar el email");
            }
        }

        public Task<Result<EmailBody>> UpdateEmailBody(UpdateEmailBodyModel model)
        {
            throw new NotImplementedException();
        }

        public Task<Result<EmailTemplate>> UpdateEmailTemplate(UpdateEmailTemplateModel model)
        {
            throw new NotImplementedException();
        }
    }
}
