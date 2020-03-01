using FATCA_Data_Preparation_and_Transmission_System.Helpers;
using FATCA_Data_Preparation_and_Transmission_System.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FATCA_Data_Preparation_and_Transmission_System.Controllers
{
    public class DecryptController : Controller
    {
        // GET: Decrypt
        public ActionResult Index()
        {
            return View();
        }
    }
}