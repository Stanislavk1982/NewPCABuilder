namespace ImageClusterizer_WPF.Models;

public class ImageCluster
{
      public int ClusterId { get; set; }
      public List<ImageVector> Images { get; set; } = new();
}

public class ClusterPosition
{
      public ImageVector Image { get; set; } = null!;
      public double X { get; set; }
      public double Y { get; set; }
      public bool IsCentroid { get; set; }
      public int ClusterId { get; set; }
}

public class ImageVisualItem
{
      public string FilePath { get; set; } = string.Empty;
      public string? ThumbnailPath { get; set; }
      public double X { get; set; }
      public double Y { get; set; }
      public int ClusterId { get; set; }
      public bool IsCentroid { get; set; }
}
