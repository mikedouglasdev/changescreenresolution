using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChangeScreenResolution
{
    class Program
    {
        static void Main(string[] args)
        {

            bool emailEnabled = false;
                
            bool.TryParse(ConfigurationManager.AppSettings["EmailEnabled"], out emailEnabled);
            string emailTo = ConfigurationManager.AppSettings["EmailTo"].ToString();

            try
            {
                int width = 1920;
                int height = 1080;
                int retries = 3;

                if (args.Length > 0)
                    int.TryParse(args[0], out width);

                if (args.Length > 1)
                    int.TryParse(args[1], out height);

                if (args.Length > 2)
                    int.TryParse(args[2], out retries);

                string emailbody = "";
                DisplaySettings set = DisplayManager.GetCurrentSettings();

                emailbody = "Original resolution = " + set.Width.ToString() + " x " + set.Height.ToString() + "<br />";
                List<DisplaySettings> displaySettings = new List<DisplaySettings>(DisplayManager.GetModesEnumerator().ToIEnumerable<DisplaySettings>());

                emailbody = emailbody + "Available Modes: <br />";
                foreach (var displaySetting in displaySettings)
                {
                    emailbody = emailbody + Environment.NewLine + "mode=" + displaySetting.Width.ToString() + " " + displaySetting.Height.ToString();
                }

                DisplaySettings after;

                while (true)
                {
                    try
                    {
                        after = ChangeDisplay(width, height, displaySettings);
                        break; // exit while loop
                    }
                    catch
                    {
                        if (--retries == 0) throw;
                        else Thread.Sleep(1000);
                    }
                }

                emailbody = emailbody + "<br />" + "New resolution = " + after.Width.ToString() + " x " + after.Height.ToString();

                if (emailEnabled)
                {
                    SendEmail(emailTo, "ChangeScreenResolution - resolution set on " + Environment.MachineName, emailbody);
                }
            }
            catch (Exception ex)
            {
                if(emailEnabled)
                {
                    string emailBody = ex.ToString();
                    SendEmail(emailTo, "ChangeScreenResolution - exception thrown on " +Environment.MachineName, emailBody);

                }
                Console.WriteLine(ex.ToString());
                
            }
        }

        private static DisplaySettings ChangeDisplay(int width, int height, List<DisplaySettings> displaySettings)
        {
            var hd = displaySettings.FirstOrDefault(d => d.Height == height && d.Width == width);

            DisplayManager.SetDisplaySettings(hd);

            DisplaySettings after = DisplayManager.GetCurrentSettings();
            return after;
        }

        static void SendEmail(string email, string subject, string body)
        {
            if (string.IsNullOrEmpty(email))
                return;

            var message = new SendGridMessage();
            string fromAddress = ConfigurationManager.AppSettings["EmailFrom"];
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];

            message.From = new MailAddress(fromAddress);
            message.AddTo(email);
            message.Subject = subject;
            message.Html = body;

            var credentials = new NetworkCredential(smtpUser, smtpPassword);

            var transportWeb = new Web(credentials);

            transportWeb.Deliver(message);
        }
    }

    public static class EnumeratorExtensions
    {
        public static IEnumerable<T> ToIEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
