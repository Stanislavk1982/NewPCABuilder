using ImageClusterizer_WPF.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace ImageClusterizer_WPF.Services;

public class OnnxService : IDisposable
{
    private const string InputName = "data";
        private readonly InferenceSession _session;

            public OnnxService(string modelPath, bool useGpu = false)
                {
                        var opts = new SessionOptions();
                                opts.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                                        opts.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
                                                opts.IntraOpNumThreads = Environment.ProcessorCount;
                                                        if (useGpu) { try { opts.AppendExecutionProvider_CUDA(0); } catch { } }
                                                                _session = new InferenceSession(modelPath, opts);
                                                                    }

                                                                        public float[] GetEmbedding(float[] pixels, VectorType vectorType)
                                                                            {
                                                                                    var tensor = new DenseTensor<float>(pixels, new[] { 1, 3, 224, 224 });
                                                                                            var inputs = new List<NamedOnnxValue>
                                                                                                        { NamedOnnxValue.CreateFromTensor(InputName, tensor) };
                                                                                                                try
                                                                                                                        {
                                                                                                                                    string outputName = vectorType == VectorType.Embedding
                                                                                                                                                    ? "resnetv24_pool1_fwd"
                                                                                                                                                                    : "resnetv24_dense0_fwd";
                                                                                                                                                                                using var results = _session.Run(inputs, new[] { outputName });
                                                                                                                                                                                            return results[0].AsEnumerable<float>().ToArray();
                                                                                                                                                                                                    }
                                                                                                                                                                                                            catch
                                                                                                                                                                                                                    {
                                                                                                                                                                                                                                using var results = _session.Run(inputs);
                                                                                                                                                                                                                                            return results[0].AsEnumerable<float>().ToArray();
                                                                                                                                                                                                                                                    }
                                                                                                                                                                                                                                                        }
                                                                                                                                                                                                                                                        
                                                                                                                                                                                                                                                            public void Dispose() => _session.Dispose();
                                                                                                                                                                                                                                                            }
