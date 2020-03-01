using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace FATCA_Data_Preparation_and_Transmission_System.Helpers.General
{
    /// <summary>
    /// This extends HttpPostedFilebase, and can be used to create an HttpPostedFile
    /// internally from the server.
    /// </summary>
    public class ServerFileBase : HttpPostedFileBase
    {
        private readonly byte[] fileBytes;

        /// <param name="fileBytes">The file in bytes</param>
        /// <param name="fileName">The name of the file (Optional)</param>
        public ServerFileBase(byte[] fileBytes, string fileName = null)
        {
            this.fileBytes = fileBytes;
            this.FileName = fileName;
            this.InputStream = new MemoryStream(fileBytes);
        }

        public override int ContentLength => fileBytes.Length;

        public override string FileName { get; }

        public override Stream InputStream { get; }
    }
}