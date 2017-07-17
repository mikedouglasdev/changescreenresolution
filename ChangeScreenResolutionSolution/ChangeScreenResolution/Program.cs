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
        /// <summary>
        /// Primary entry point of console application.  This application should be called with width and height arguments on computer startup
        /// In Windows 10, use Start > Run > shell:common startup to open folder (C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup)
        /// create shortcut for app and copy to folder
        /// add arguments
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            bool emailEnabled = false;
                
            bool.TryParse(ConfigurationManager.AppSettings["EmailEnabled"], out emailEnabled);
            string emailTo = ConfigurationManager.AppSettings["EmailTo"].ToString();

            try
            {
                string message;
                int width = 1920;
                int height = 1080;
                int retries = 10;

                if (args.Length > 0)
                    int.TryParse(args[0], out width);

                if (args.Length > 1)
                    int.TryParse(args[1], out height);

                if (args.Length > 2)
                    int.TryParse(args[2], out retries);

                string emailbody = "";
                DisplaySettings set = DisplayManager.GetCurrentSettings();

                message = "Original resolution = " + set.Width.ToString() + " x " + set.Height.ToString() + "<br />";
                LogMessage(message);
                emailbody = message;

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
                        LogMessage("Attempting to change display settings to " + width + " x " + height + ".  Retries remaining " + (retries - 1).ToString());
                        after = ChangeDisplay(width, height, displaySettings);
                        break; // exit while loop
                    }
                    catch(Exception ex)
                    {

                        if (--retries == 0)
                        {
                            LogMessage("Change Screen Resolution retry failed. No more attempts.");
                            throw;
                        }
                        else
                        {
                            LogMessage("Change Screen Resolution retry failed. Retrying in 1 second.");
                            Thread.Sleep(1000);
                        }
                    }
                }

                emailbody = emailbody + "<br />" + "New resolution = " + after.Width.ToString() + " x " + after.Height.ToString();

                message = "ChangeScreenResolution - resolution set on " + Environment.MachineName;
                LogMessage(message);

                if (emailEnabled)
                {
                    SendEmail(emailTo, message, emailbody);
                }
            }
            catch (Exception ex)
            {
                if(emailEnabled)
                {
                    string emailBody = ex.ToString();
                    string title = "ChangeScreenResolution - exception thrown on " + Environment.MachineName;
                    LogMessage(title + " - " + emailBody);
                    SendEmail(emailTo, title, emailBody);

                }
                Console.WriteLine(ex.ToString());
                
            }
        }

        /// <summary>
        /// Method that wraps win 32 api call to change the display settings
        /// </summary>
        /// <param name="width">number of pixels wide such as 1920</param>
        /// <param name="height">number of pixels tall such as 1080</param>
        /// <param name="displaySettings">List of available screen modes supported</param>
        /// <returns></returns>
        private static DisplaySettings ChangeDisplay(int width, int height, List<DisplaySettings> displaySettings)
        {
            var hd = displaySettings.FirstOrDefault(d => d.Height == height && d.Width == width);

            DisplayManager.SetDisplaySettings(hd);

            DisplaySettings after = DisplayManager.GetCurrentSettings();
            return after;
        }

        /// <summary>
        /// Can send email notification using SendGrid
        /// </summary>
        /// <param name="email">email recipient(s)</param>
        /// <param name="subject">title of the email</param>
        /// <param name="body">details of the email</param>
        private static void SendEmail(string email, string subject, string body)
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

        /// <summary>
        /// Can Log Message to a local file.  Can be toggled on or off from config file.
        /// </summary>
        /// <param name="message">Details of what will be logged</param>
        private static void LogMessage(string message)
        {
            if(ConfigurationManager.AppSettings["LoggingEnabled"] == null || ConfigurationManager.AppSettings["LoggingEnabled"].ToLower() != "true")
            {
                return;
            }
            string filename = DateTime.Today.ToString("yyyy-MM-dd") + "_log.txt";

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename, true))
            {
                file.WriteLine(DateTime.Now.ToString("hh:mm:ss") + " - " + message);
            }
        }
    }

    /// <summary>
    /// Extension method that returns list
    /// </summary>
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
