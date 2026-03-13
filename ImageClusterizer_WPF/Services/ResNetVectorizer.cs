using ImageClusterizer_WPF.Models;
using ImageClusterizer_WPF.Utility;

namespace ImageClusterizer_WPF.Services;

public interface IVectorService
{
      Task<float[]> GetEmbeddingAsync(string imagePath, VectorType vectorType);
}

public class ResNetVectorizer : IVectorService, IDisposable
{
    private readonly OnnxService _onnx;

        public ResNetVectorizer(string modelPath, bool useGpu = false)
            {
                    _onnx = new OnnxService(modelPath, useGpu);
                        }

                            public Task<float[]> GetEmbeddingAsync(string imagePath, VectorType vectorType)
                                {
                                        return Task.Run(() =>
                                                {
                                                            var (pixels, resized) = ImagePreprocessor.Preprocess(imagePath);
                                                                        resized.Dispose();
                                                                                    return _onnx.GetEmbedding(pixels, vectorType);
                                                                                            });
                                                                                                }

                                                                                                    public void Dispose() => _onnx.Dispose();
                                                                                                    }
