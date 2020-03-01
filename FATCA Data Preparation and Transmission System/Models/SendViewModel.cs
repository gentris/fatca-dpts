using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FATCA_Data_Preparation_and_Transmission_System.Models
{
    public class SendViewModel
    {
        [Required]
        public HttpPostedFileBase EncryptedPackage { get; set; }

        [Required]
        public string SFTPServer { get; set; }
        [Required]
        public string SFTPUsername { get; set; }
        [Required]
        public string SFTPPassword { get; set; }

        [Required]
        public string PackagePath { get; set; }
    }
}