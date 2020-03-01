using FATCA_Data_Preparation_and_Transmission_System.Models;
using System;
using System.Web.Mvc;
using System.IO;
using FATCA_Data_Preparation_and_Transmission_System.Helpers;
using System.Net;

namespace FATCA_Data_Preparation_and_Transmission_System.Controllers
{
    public class EncryptController : Controller
    {
        // GET: Encrypt
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(EncryptViewModel encryption)
        {
            if (ModelState.IsValid)
            {
                // Paths for each file that we are going to store on disk
                string mainDirectory = Server.MapPath("~/App_Data/Encrypt/");
                string certificatesDirectory = Server.MapPath("~/App_Data/Certificates/");
                string subDir = DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") + "\\";
                string encryptedDirectory = Path.Combine(mainDirectory, subDir);

                string xmlReportPath = Path.Combine(encryptedDirectory, encryption.Report.FileName);
                string senderCertificatePath = Path.Combine(certificatesDirectory, "sender.pfx");
                string receiverCertificatePath = Path.Combine(certificatesDirectory, "receiver.cer");
                string hctaCertificatePath = ""; // Optional

                // Create subdirectory
                Directory.CreateDirectory(encryptedDirectory);

                // Save xml report to the disk
                encryption.Report.SaveAs(xmlReportPath);

                // Read certificates
                byte[] senderCertificate;
                using (Stream inputStream = encryption.SenderCertificate.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }
                    senderCertificate = memoryStream.ToArray();
                }

                byte[] irsCertificate;
                using (Stream inputStream = encryption.IRSCertificate.InputStream)
                {
                    MemoryStream memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }
                    irsCertificate = memoryStream.ToArray();
                }

                byte[] hctaCertificate = { };
                if (encryption.Model1Option2)
                {
                    using (Stream inputStream = encryption.HCTACertificate.InputStream)
                    {
                        MemoryStream memoryStream = inputStream as MemoryStream;
                        if (memoryStream == null)
                        {
                            memoryStream = new MemoryStream();
                            inputStream.CopyTo(memoryStream);
                        }
                        hctaCertificate = memoryStream.ToArray();
                    }
                }

                // Perform the schema validation
                //string validationError = XmlManager.CheckSchema(xmlReportPath, "Schemas folder");

                //if (validationError != "")
                //{
                //   return Json(new { StatusCode = HttpStatusCode.InternalServerError, errorMessage = "Schema validation error: " + validationError });
                //}

                // Load XML report content
                byte[] xmlContent = System.IO.File.ReadAllBytes(xmlReportPath);
                string senderGIIN = Path.GetFileNameWithoutExtension(encryption.Report.FileName);
                string fileExtension = Path.GetExtension(encryption.Report.FileName.ToUpper()).Replace(".", "");

                // Perform signature
                byte[] signedReport = XmlManager.Sign(XmlSignatureType.Enveloping, xmlContent, senderCertificate, encryption.SenderCertPassword, xmlReportPath);
                string signedReportPath = xmlReportPath.Replace(".xml", "_Payload.xml");

                string zipPackagePath = signedReportPath.Replace(".xml", ".zip");

                // Save signed xml report to the disk
                System.IO.File.WriteAllBytes(signedReportPath, signedReport);

                // Create the zip archive that will hold the generated files, and add the signed report to it
                ZipManager.CreateArchive(signedReportPath, zipPackagePath);

                // Generate AES key (32 bytes) & default initialization vector (empty)
                byte[] aesEncryptionKey = AesManager.GenerateRandomKey(AesManager.KeySize / 8);
                byte[] aesEncryptionVector = AesManager.GenerateRandomKey(16, false);

                // Encrypt file and save to disk
                string encryptedFilePath = zipPackagePath.Replace(".zip", "");
                string encryptedHCTAFilePath = zipPackagePath.Replace(".zip", "");
                string payloadFilePath = encryptedFilePath + "";
                AesManager.EncryptFile(zipPackagePath, encryptedFilePath, aesEncryptionKey, aesEncryptionVector, false);

                // Encrypt AES key with the public key of the certificate and save to disk
                encryptedFilePath = Path.GetDirectoryName(zipPackagePath) + "\\000000.00000.TA.840_Key";
                AesManager.EncryptAesKey(aesEncryptionKey, aesEncryptionVector, irsCertificate, "yourpasswordhere", encryptedFilePath, false);

                //For Model1 Option2 Only, encrypt the AES Key with the HCTA Public Key
                if (encryption.Model1Option2)
                {
                    encryptedHCTAFilePath = Path.GetDirectoryName(zipPackagePath) + "\\000000.00000.TA." + "901" + "_Key";
                    AesManager.EncryptAesKey(aesEncryptionKey, aesEncryptionVector, hctaCertificate, "", encryptedHCTAFilePath, false);
                }

                // cleanup
                signedReport = null;
                aesEncryptionKey = aesEncryptionVector = null;

                string packageName = "";
                try
                {
                    DateTime uDat = new DateTime();
                    uDat = DateTime.UtcNow;
                    packageName = uDat.ToString("yyyyMMddTHHmmssfffZ") + "_" + senderGIIN;
                    string metadataFilePath = Path.Combine(encryptedDirectory, senderGIIN + "_Metadata.xml");
                    XmlManager.CreateMetadataFile(metadataFilePath, fileExtension, true, "2018", senderGIIN, packageName);
                    
                    // Check the signature to make sure it is valid, this requires the KeyInfo to be present
                    bool result = XmlManager.CheckSignature(signedReportPath);
                    if (result == false)
                    {
                        return Json(new { statusCode = HttpStatusCode.InternalServerError, errorMessage = "The document signature is not valid!" });
                    }

                    //Add the metadata, payload, and key files to the final zip package
                    // Add signed file to the zip package
                    ZipManager.CreateArchive(metadataFilePath, encryptedDirectory + "\\" + packageName + ".zip");
                    ZipManager.UpdateArchive(encryptedFilePath, encryptedDirectory + "\\" + packageName + ".zip");
                    ZipManager.UpdateArchive(payloadFilePath, encryptedDirectory + "\\" + packageName + ".zip");
                    
                    //Add the HCTA Key file for a Model 1 Option 2 packet
                    if (encryption.Model1Option2)
                    {
                        ZipManager.UpdateArchive(encryptedHCTAFilePath, encryptedDirectory + "\\" + packageName + ".zip");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                encryption.PackagePath = encryptedDirectory + "\\" + packageName + ".zip";

                return RedirectToAction("Download", new { packagePath = encryptedDirectory + "\\" + packageName + ".zip" });
            } else
            {
                return View(encryption);
            }
        }

        public ActionResult Download(string packagePath)
        {
            ViewBag.PackagePath = packagePath;
            return View();
        }

        public FileStreamResult DownloadPackage(string packagePath)
        {
            string packageName = Path.GetFileName(packagePath);
            FileStream fileStream = new FileStream(packagePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096);
            return File(fileStream, System.Net.Mime.MediaTypeNames.Application.Octet, packageName);
        }

    }
}