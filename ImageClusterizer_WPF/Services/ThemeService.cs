using System.IO;
using System.Text.Json;
using System.Windows;

namespace ImageClusterizer_WPF.Services;

public class ThemeService
{
      public enum Theme { Light, Dark }

      private static readonly string SettingsPath =
                Path.Combine(AppContext.BaseDirectory, "AppSettings.json");

      public Theme CurrentTheme { get; private set; } = Theme.Light;

      public void ToggleTheme() =>
                ApplyTheme(CurrentTheme == Theme.Light ? Theme.Dark : Theme.Light);

      public void ApplyTheme(Theme theme)
      {
                CurrentTheme = theme;
                var merged = Application.Current.Resources.MergedDictionaries;
                var old = merged.FirstOrDefault(d => d.Source?.ToString().Contains("Theme") == true);
                if (old != null) merged.Remove(old);
                var uri = theme == Theme.Dark
                              ? new Uri("pack://application:,,,/Themes/DarkTheme.xaml")
                              : new Uri("pack://application:,,,/Themes/LightTheme.xaml");
                merged.Add(new ResourceDictionary { Source = uri });
      }

      public void SavePreference()
      {
                try
                {
                              var settings = LoadSettings();
                              settings["Theme"] = CurrentTheme.ToString();
                              File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings,
                                                                                                       new JsonSerializerOptions { WriteIndented = true }));
                }
                catch { }
      }

      public void LoadPreference()
      {
                try
                {
                              var settings = LoadSettings();
                              if (settings.TryGetValue("Theme", out var themeStr) &&
                                                  Enum.TryParse<Theme>(themeStr?.ToString(), out var theme))
                                                ApplyTheme(theme);
                }
                catch { }
      }

      private static Dictionary<string, object?> LoadSettings()
      {
                if (!File.Exists(SettingsPath))
                              return new Dictionary<string, object?>();
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<Dictionary<string, object?>>(json)
                                 ?? new Dictionary<string, object?>();
      }
}
