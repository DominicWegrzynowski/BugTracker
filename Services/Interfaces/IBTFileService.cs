using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BugTracker.Services.Interfaces
{
    public interface IBTFileService
    {
        Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);
        string ConvertByteArrayToFile(byte[] fileData, string extension);
        string GetFileIcon(string file);
        string FormatFileSize(long bytes);
    }
}
