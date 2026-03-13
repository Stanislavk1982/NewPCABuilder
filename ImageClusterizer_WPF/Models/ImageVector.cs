namespace ImageClusterizer_WPF.Models;

public record ImageVector
{
    public string FilePath { get; init; } = string.Empty;
        public float[] Vector { get; init; } = Array.Empty<float>();
            public VectorType VectorType { get; init; } = VectorType.Logit;
                public long FileSize { get; init; }
                    public string? ThumbnailPath { get; init; }
                        public float? PcaX { get; init; }
                            public float? PcaY { get; init; }
                            }

public enum VectorType { Logit, Embedding }
