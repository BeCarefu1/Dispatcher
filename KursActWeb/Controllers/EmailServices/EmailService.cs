using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using ActGIPelectroWeb.ViewModels;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ActGIPelectroWeb.EmailServices
{
    public class EmailService
    {
        //Отправить без файла
        public async Task SendEmailAsync(List<string> emailList, string subject, string message)
        {
            await SendEmailAsync(emailList, subject, message, null, null);
        }

        //Отправить с прикрепленным файлом
        public async Task SendEmailAsync(List<string> emailList, string subject, string message, byte[] fileData, string fileName)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Почтовый робот ООО КУРС", "noreply@kursufa.ru"));
            //Добавим список адресов
            emailList.ForEach(email => emailMessage.To.Add(new MailboxAddress("", email)));
            emailMessage.Subject = subject;
            //Контент письма
            var multipart = new Multipart("mixed");
            //Тело сообщения (HTML разметка)
            multipart.Add(new TextPart(MimeKit.Text.TextFormat.Text)
            {
                Text = message
            });
            MemoryStream stream = new MemoryStream();
            //Файл
            if (fileName != null)
            {
                stream = new MemoryStream(fileData);
                var attachment = new MimePart()
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = fileName
                };
                multipart.Add(attachment);
            }
            emailMessage.Body = multipart;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.yandex.ru", 25, false);
                await client.AuthenticateAsync("noreply@kursufa.ru", "E920am02");
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
            stream.Dispose();
        }
    }
}
