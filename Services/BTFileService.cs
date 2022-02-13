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
            decimal fileSize = bytes;

            while (Math.Round(fileSize / 1024) >= 1)
            {
                fileSize /= bytes;
                counter++;
            }

            return String.Format("{0:n1}{1}", fileSize, suffixes[counter]);

        }
        #endregion

        #region Get File Icon
        public string GetFileIcon(string file)
        {
            string fileImage = "default";

            if (!string.IsNullOrWhiteSpace(file))
            {
                fileImage = Path.GetExtension(file).Replace(".", "");
                return $"/img/png/{fileImage}.png";
            }

            return fileImage;
        } 
        #endregion
    }
}
