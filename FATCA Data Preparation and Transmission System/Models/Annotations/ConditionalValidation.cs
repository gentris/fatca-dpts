using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FATCA_Data_Preparation_and_Transmission_System.Models.Annotations
{
    /// <summary>
    /// Offers custom validation based on the boolean condition given as an input
    /// return
    /// </summary>
    public class ConditionalValidation : ValidationAttribute
    {
        public bool BooleanCondition { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (this.BooleanCondition)
                return ValidationResult.Success;
            else
                return new ValidationResult(this.ErrorMessage);
        }
    }
}