using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageClusterizer_WPF.Utility;

public static class ImagePreprocessor
{
      private static readonly float[] Mean = { 0.485f, 0.456f, 0.406f };
      private static readonly float[] Std  = { 0.229f, 0.224f, 0.225f };

      public static (float[] pixels, Image<Rgb24> resized) Preprocess(string imagePath)
      {
                var original = Image.Load<Rgb24>(imagePath);
                var resized  = original.Clone(ctx => ctx.Resize(224, 224));
                original.Dispose();

                var pixels = new float[3 * 224 * 224];
                resized.ProcessPixelRows(accessor =>
                                         {
                                                       for (int y = 0; y < 224; y++)
                                                       {
                                                                         var row = accessor.GetRowSpan(y);
                                                                         for (int x = 0; x < 224; x++)
                                                                         {
                                                                                               var px = row[x];
                                                                                               int idx = y * 224 + x;
                                                                                               pixels[0 * 224 * 224 + idx] = (px.R / 255f - Mean[0]) / Std[0];
                                                                                               pixels[1 * 224 * 224 + idx] = (px.G / 255f - Mean[1]) / Std[1];
                                                                                               pixels[2 * 224 * 224 + idx] = (px.B / 255f - Mean[2]) / Std[2];
                                                                         }
                                                       }
                                         });

                return (pixels, resized);
      }
}
