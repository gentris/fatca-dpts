using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FATCA_Data_Preparation_and_Transmission_System.Models
{
    public class NotificationsViewModel
    {
        public HttpPostedFileBase EncryptedNotification { get; set; }
        public HttpPostedFileBase SenderCertificate { get; set; }
        public string SenderCertPassword { get; set; }
    }
}