using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ESD_Project.Models.Code
{
    public class FileUploadService
    {
        public byte[] ConvertToBytes(HttpPostedFileBase file)
        {
            byte[] Bytes = null;
            BinaryReader reader = new BinaryReader(file.InputStream);
            Bytes = reader.ReadBytes((int)file.ContentLength);
            return Bytes;
        }
    }
}