using System.Threading.Channels;
using ImageClusterizer_WPF.Models;
using ImageClusterizer_WPF.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageClusterizer_WPF.Services;

public class ImageScanner
{
      private readonly IVectorService _vectorizer;
      private readonly IVectorDatabase _db;
      private readonly StorageService _storage;
      private static readonly string[] Exts = { ".jpg", ".jpeg", ".png", ".bmp" };

      public ImageScanner(IVectorService vectorizer, IVectorDatabase db, StorageService storage)
      {
                _vectorizer = vectorizer; _db = db; _storage = storage;
      }

      public async Task ScanAsync(string folderPath, VectorType vectorType,
                                          IProgress<(int current, int total, string file)>? progress = null,
                                          CancellationToken ct = default)
      {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                              .Where(f => Exts.Contains(Path.GetExtension(f).ToLowerInvariant())).ToList();

                var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(32) { FullMode = BoundedChannelFullMode.Wait });

                var producer = Task.Run(async () =>
                                        {
                                                      foreach (var file in files) await channel.Writer.WriteAsync(file, ct);
                                                      channel.Writer.Complete();
                                        }, ct);

                int processed = 0;
                var consumer = Task.Run(async () =>
                                        {
                                                      await foreach (var file in channel.Reader.ReadAllAsync(ct))
                                                      {
                                                                        ct.ThrowIfCancellationRequested();
                                                                        try
                                                                        {
                                                                                              var existing = (await _db.GetAllAsync()).FirstOrDefault(v => v.FilePath == file);
                                                                                              float[] vector;
                                                                                              if (existing?.Vector.Length > 0) vector = existing.Vector;
                                                                                              else vector = await _vectorizer.GetEmbeddingAsync(file, vectorType);

                                                                                              var thumbPath = _storage.GetThumbnailPath(file);
                                                                                              if (!File.Exists(thumbPath))
                                                                                              {
                                                                                                                        await Task.Run(() =>
                                                                                                                                       {
                                                                                                                                                                     using var img = Image.Load<Rgb24>(file);
                                                                                                                                                                     img.Mutate(x => x.Resize(224, 224));
                                                                                                                                                                     img.Save(thumbPath, new JpegEncoder { Quality = 85 });
                                                                                                                                       });
                                                                                              }

                                                                                              var iv = new ImageVector
                                                                                              {
                                                                                                                        FilePath = file, Vector = VectorHelper.L2Normalize(vector),
                                                                                                                        VectorType = vectorType, FileSize = new FileInfo(file).Length,
                                                                                                                        ThumbnailPath = thumbPath, PcaX = existing?.PcaX, PcaY = existing?.PcaY,
                                                                                              };
                                                                                              await _db.SaveAsync(iv);
                                                                        }
                                                                        catch { }
                                                                        processed++;
                                                                        progress?.Report((processed, files.Count, file));
                                                      }
                                        }, ct);

                await Task.WhenAll(producer, consumer);
      }
}
