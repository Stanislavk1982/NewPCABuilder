using System.Security.Cryptography;
using System.Text;

namespace ImageClusterizer_WPF.Services;

public class StorageService
{
      private readonly string _baseDir;

      public StorageService()
      {
                _baseDir = Path.Combine(AppContext.BaseDirectory, "data");
                Directory.CreateDirectory(_baseDir);
                Directory.CreateDirectory(ThumbnailsFolder);
      }

      public string DatabasePath => Path.Combine(_baseDir, "vectors.db");
      public string ThumbnailsFolder => Path.Combine(_baseDir, "thumbnails");

      public string GetThumbnailPath(string imageFilePath)
      {
                var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(imageFilePath)));
                return Path.Combine(ThumbnailsFolder, hash + ".jpg");
      }

      public async Task ClearAllDataAsync()
      {
                await Task.Run(() =>
                               {
                                             if (File.Exists(DatabasePath)) File.Delete(DatabasePath);
                                             foreach (var f in Directory.GetFiles(ThumbnailsFolder))
                                                               File.Delete(f);
                               });
      }
}
