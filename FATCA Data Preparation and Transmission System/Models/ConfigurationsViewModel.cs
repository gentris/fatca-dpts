using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FATCA_Data_Preparation_and_Transmission_System.Models
{
    public class ConfigurationsViewModel
    {
        [Required]
        public HttpPostedFileBase Report { get; set; }

        [Required]
        public HttpPostedFileBase SenderCertificate { get; set; }

        [DataType(DataType.Password)]
        public string SenderCertPassword { get; set; }

        [Required]
        public HttpPostedFileBase IRSCertificate { get; set; }

        [DataType(DataType.Password)]
        public string IRSCertPassword { get; set; }

        public bool Model1Option2 { get; set; }

        //[ConditionalValidation(BooleanCondition = true, ErrorMessage = "asdf")]
        public HttpPostedFileBase HCTACertificate { get; set; }

        //[DataType(DataType.Password)]
        public string HCTACertPassword { get; set; }
    }
}