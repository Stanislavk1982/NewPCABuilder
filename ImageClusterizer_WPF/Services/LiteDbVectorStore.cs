using ImageClusterizer_WPF.Models;
using LiteDB;

namespace ImageClusterizer_WPF.Services;

public class LiteDbVectorStore : IVectorDatabase, IDisposable
{
    private readonly LiteDatabase _db;
        private readonly ILiteCollection<ImageVectorEntity> _col;
            private readonly string _dbPath;

                public LiteDbVectorStore(string dbPath)
                    {
                            _dbPath = dbPath;
                                    _db = new LiteDatabase(dbPath);
                                            _col = _db.GetCollection<ImageVectorEntity>("vectors");
                                                    _col.EnsureIndex(x => x.FilePath, unique: true);
                                                        }

                                                            public Task SaveAsync(ImageVector v) => Task.Run(() => _col.Upsert(ToEntity(v)));

                                                                public Task SaveBatchAsync(IEnumerable<ImageVector> vectors) => Task.Run(() =>
                                                                    {
                                                                            _db.BeginTrans();
                                                                                    try { foreach (var v in vectors) _col.Upsert(ToEntity(v)); _db.Commit(); }
                                                                                            catch { _db.Rollback(); throw; }
                                                                                                });

                                                                                                    public Task<List<ImageVector>> GetAllAsync() =>
                                                                                                            Task.Run(() => _col.FindAll().Select(FromEntity).ToList());
                                                                                                            
                                                                                                                public Task SavePcaCoordinatesAsync(string filePath, float pcaX, float pcaY) => Task.Run(() =>
                                                                                                                    {
                                                                                                                            var e = _col.FindOne(x => x.FilePath == filePath);
                                                                                                                                    if (e == null) return;
                                                                                                                                            e.PcaX = pcaX; e.PcaY = pcaY; _col.Update(e);
                                                                                                                                                });
                                                                                                                                                
                                                                                                                                                    public Task ClearPcaCacheAsync() => Task.Run(() =>
                                                                                                                                                            _col.UpdateMany(v => { v.PcaX = null; v.PcaY = null; return v; }, _ => true));
                                                                                                                                                            
                                                                                                                                                                public Task ClearAllAsync() => Task.Run(() => _col.DeleteAll());
                                                                                                                                                                
                                                                                                                                                                    public long GetDatabaseSize()
                                                                                                                                                                        {
                                                                                                                                                                                try { return new FileInfo(_dbPath).Length; } catch { return 0; }
                                                                                                                                                                                    }
                                                                                                                                                                                    
                                                                                                                                                                                        private static ImageVectorEntity ToEntity(ImageVector v) => new()
                                                                                                                                                                                            {
                                                                                                                                                                                                    FilePath = v.FilePath, VectorType = v.VectorType, FileSize = v.FileSize,
                                                                                                                                                                                                            VectorData = ToBytes(v.Vector), ThumbnailPath = v.ThumbnailPath,
                                                                                                                                                                                                                    PcaX = v.PcaX, PcaY = v.PcaY
                                                                                                                                                                                                                        };
                                                                                                                                                                                                                        
                                                                                                                                                                                                                            private static ImageVector FromEntity(ImageVectorEntity e) => new()
                                                                                                                                                                                                                                {
                                                                                                                                                                                                                                        FilePath = e.FilePath, VectorType = e.VectorType, FileSize = e.FileSize,
                                                                                                                                                                                                                                                Vector = FromBytes(e.VectorData), ThumbnailPath = e.ThumbnailPath,
                                                                                                                                                                                                                                                        PcaX = e.PcaX, PcaY = e.PcaY
                                                                                                                                                                                                                                                            };
                                                                                                                                                                                                                                                            
                                                                                                                                                                                                                                                                private static byte[] ToBytes(float[] v) { var b = new byte[v.Length * 4]; Buffer.BlockCopy(v, 0, b, 0, b.Length); return b; }
                                                                                                                                                                                                                                                                    private static float[] FromBytes(byte[] b) { var v = new float[b.Length / 4]; Buffer.BlockCopy(b, 0, v, 0, b.Length); return v; }
                                                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                                        public void Dispose() => _db.Dispose();
                                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                                        
public class ImageVectorEntity
{
      public int Id { get; set; }
      public string FilePath { get; set; } = string.Empty;
      public VectorType VectorType { get; set; }
      public long FileSize { get; set; }
      public byte[] VectorData { get; set; } = Array.Empty<byte>();
      public string? ThumbnailPath { get; set; }
      public float? PcaX { get; set; }
      public float? PcaY { get; set; }
}
