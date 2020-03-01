using FATCA_Data_Preparation_and_Transmission_System.Models;
using System;
using System.Web.Mvc;
using System.IO;
using WinSCP;
using FATCA_Data_Preparation_and_Transmission_System.Helpers;

namespace FATCA_Data_Preparation_and_Transmission_System.Controllers
{
    public class SendController : Controller
    {
        // GET: Send
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string packagePath)
        {
            ViewBag.PackagePath = packagePath;
            return View();
        }

        public ActionResult Send(SendViewModel sendView, string packagePath)
        {
            if (!string.IsNullOrEmpty(sendView.PackagePath))
            {
                ModelState["EncryptedPackage"].Errors.Clear();
                if (ModelState.IsValid)
                {
                    // Setup sessions options
                    SessionOptions currentSFTPSession = SFTPManager.CreateSFTPSession(sendView.SFTPServer, sendView.SFTPUsername, sendView.SFTPPassword);
                    string sftpUpName = packagePath;

                    try
                    {
                        string transferResult = SFTPManager.UploadFile(currentSFTPSession, sftpUpName);
                        ViewBag.Message = transferResult;
                    } 
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;
                    }

                    return View("~/Views/Shared/Info.cshtml");
                }
                else
                {
                    return View(sendView);
                }
            }
            else
            {
                // File got uploaded from the user
                ModelState["PackagePath"].Errors.Clear();
                if (ModelState.IsValid)
                {
                    string dir = Server.MapPath("~/App_Data/Send/");
                    string tempPackagePath = Path.Combine(dir, sendView.EncryptedPackage.FileName);

                    // Save xml report to the disk
                    sendView.EncryptedPackage.SaveAs(tempPackagePath);

                    // Setup sessions options
                    SessionOptions currentSFTPSession = SFTPManager.CreateSFTPSession(sendView.SFTPServer, sendView.SFTPUsername, sendView.SFTPPassword);
                    string sftpUpName = tempPackagePath;

                    try
                    {
                        string transferResult = SFTPManager.UploadFile(currentSFTPSession, sftpUpName);
                        ViewBag.Message = transferResult;
                    }
                    catch (Exception e)
                    {
                        ViewBag.Message = e.Message;
                    }

                    return View("~/Views/Shared/Info.cshtml");
                }
                else
                {
                    return View(sendView);
                }
            }
        }
    }
}