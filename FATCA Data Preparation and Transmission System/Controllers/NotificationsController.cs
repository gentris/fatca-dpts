using FATCA_Data_Preparation_and_Transmission_System.Helpers;
using FATCA_Data_Preparation_and_Transmission_System.Models;
using System;
using System.IO;
using System.Web.Mvc;

namespace FATCA_Data_Preparation_and_Transmission_System.Controllers
{
    public class NotificationsController : Controller
    {
        // GET: Notifications
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Decrypt(NotificationsViewModel notification)
        {
            if (ModelState.IsValid)
            {
                // Paths for each file that we are going to store on disk
                string mainDirectory = Server.MapPath("~/App_Data/Decrypt/");
                string certificatesDirectory = Server.MapPath("~/App_Data/Certificates/");
                string subDir = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "\\";
                string decryptedDirectory = Path.Combine(mainDirectory, subDir);
                string encryptedNotificationPath = Path.Combine(decryptedDirectory, notification.EncryptedNotification.FileName);
                string senderCertificatePath = Path.Combine(certificatesDirectory, "sender.pfx");

                //Create subdirectory
                Directory.CreateDirectory(decryptedDirectory);

                // Save the files to disk
                notification.EncryptedNotification.SaveAs(encryptedNotificationPath);

                // Read certificate
                byte[] senderCertificate;
                using (Stream inputStream = notification.SenderCertificate.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }
                    senderCertificate = memoryStream.ToArray();
                }

                string zipFolder = "";
                try
                {
                    //Deflate the zip archive
                    zipFolder = ZipManager.ExtractArchive(encryptedNotificationPath, decryptedDirectory);
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return View("~/Views/Shared/Info.cshtml");
                }

                //Decrypt the Payload
                string decryptedNotificationPath = "";
                try
                {
                    decryptedNotificationPath = AesManager.DecryptNotification(zipFolder, senderCertificate, notification.SenderCertPassword, false);
                }
                catch (Exception e)
                {
                    ViewBag.Message = e.Message;
                    return View("~/Views/Shared/Info.cshtml");
                }

                ViewBag.DecryptedNotificationPath = decryptedNotificationPath;
                ViewBag.DecryptedNotificationXML = System.IO.File.ReadAllText(decryptedNotificationPath);
                return View("~/Views/Notifications/Download.cshtml");
            }
            else
            {
                return View(notification);
            }
        }
    }
}