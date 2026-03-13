using Microsoft.ML.OnnxRuntime;

namespace ImageClusterizer_WPF.Utility;

public static class GpuDetector
{
      public record GpuInfo(bool IsAvailable, string ProviderName, string DeviceName);

      public static GpuInfo Detect()
      {
                try
                {
                              var providers = OrtEnv.Instance().GetAvailableProviders();
                              if (providers.Contains("CUDAExecutionProvider"))
                                                return new GpuInfo(true, "CUDA", "CUDA GPU");
                              if (providers.Contains("DmlExecutionProvider"))
                                                return new GpuInfo(true, "DirectML", "GPU (DirectML)");
                }
                catch { }
                return new GpuInfo(false, "CPU", "No GPU detected");
      }
}
