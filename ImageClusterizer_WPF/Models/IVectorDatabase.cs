namespace ImageClusterizer_WPF.Models;

public interface IVectorDatabase
{
      Task SaveAsync(ImageVector vector);
      Task SaveBatchAsync(IEnumerable<ImageVector> vectors);
      Task<List<ImageVector>> GetAllAsync();
      Task SavePcaCoordinatesAsync(string filePath, float pcaX, float pcaY);
      Task ClearPcaCacheAsync();
      Task ClearAllAsync();
      long GetDatabaseSize();
}
