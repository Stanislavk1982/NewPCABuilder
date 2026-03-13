namespace ImageClusterizer_WPF.Services;

public class LogService
{
      private readonly object _lock = new();
      public event Action<string?>? LogAdded;

      public void Log(string message)
      {
                var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                lock (_lock) { LogAdded?.Invoke(line); }
      }

      public void Clear()
      {
                lock (_lock) { LogAdded?.Invoke(null); }
      }
}
