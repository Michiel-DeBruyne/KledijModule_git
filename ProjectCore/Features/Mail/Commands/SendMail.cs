using MediatR;
using ProjectCore.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCore.Features.Mail.Commands
{
    public class SendMail
    {
        public record Command : IRequest<Result>
        {
            public string ToEmail { get; set; }
            public string FromEmail { get; set; }
            public string Subject { get; set; }

            public string Body { get; set; } 

        }

        public class CommandHandler : IRequestHandler<Command, Result>
        {
            private readonly string _smtpServer;
            private readonly int _port;
            
            public CommandHandler()
            {
                _smtpServer = "mail1.fluviusnet.be";
                _port = 25;
            }
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                try
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(request.FromEmail),
                        Subject = request.Subject,
                        Body = request.Body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(request.ToEmail);

                    using (var smtpClient = new SmtpClient(_smtpServer, _port))
                    {
                        await smtpClient.SendMailAsync(mailMessage);
                    }
                    return new SuccessResult();
                }catch(Exception ex) {
                    return new ErrorResult(ex.Message);
                }
            }
        }

    }
}
