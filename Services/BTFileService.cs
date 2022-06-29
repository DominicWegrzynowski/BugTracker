using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace BugTracker.Services
{
    public class BTFileService : IBTFileService
    {
        #region Fields
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        #endregion

        #region Convert Byte Array To File
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData);
                return string.Format($"data:{extension};base64,{imageBase64Data}");
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Convert File To Byte Array
        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();

                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Format File Size
        public string FormatFileSize(long bytes)
        {
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }

            return string.Format("{0:n1}{1}", number, suffixes[counter]);
        }
        #endregion

        #region Get File Icon
        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            string fileImage = $"/images/contenttype/{ext}.png";
            return fileImage;
        } 
        #endregion
    }
}
