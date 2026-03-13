namespace ImageClusterizer_WPF.Utility;

public static class VectorHelper
{
      public static byte[] ToBytes(float[] v)
      {
                var b = new byte[v.Length * sizeof(float)];
                Buffer.BlockCopy(v, 0, b, 0, b.Length);
                return b;
      }

      public static float[] FromBytes(byte[] b)
      {
                var v = new float[b.Length / sizeof(float)];
                Buffer.BlockCopy(b, 0, v, 0, b.Length);
                return v;
      }

      public static float[] L2Normalize(float[] v)
      {
                float norm = MathF.Sqrt(v.Sum(x => x * x));
                if (norm < 1e-10f) return v;
                return v.Select(x => x / norm).ToArray();
      }
}
