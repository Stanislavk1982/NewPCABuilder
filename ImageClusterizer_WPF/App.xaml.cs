using System.Windows;
using ImageClusterizer_WPF.Models;
using ImageClusterizer_WPF.Services;
using ImageClusterizer_WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ImageClusterizer_WPF;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
            {
                    base.OnStartup(e);

                            var sc = new ServiceCollection();

                                    // Core services
                                            var storage = new StorageService();
                                                    sc.AddSingleton(storage);
                                                            sc.AddSingleton<IVectorDatabase>(new LiteDbVectorStore(storage.DatabasePath));
                                                                    sc.AddSingleton<LogService>();
                                                                            sc.AddSingleton<ThemeService>();
                                                                                    sc.AddSingleton<ClusteringService>();

                                                                                            // Model path — place resnet50-v2-7.onnx in Models/ONNX/ folder
                                                                                                    string modelPath = Path.Combine(AppContext.BaseDirectory, "Models", "ONNX", "resnet50-v2-7.onnx");
                                                                                                            sc.AddSingleton<IVectorService>(sp =>
                                                                                                                        new ResNetVectorizer(modelPath, useGpu: false));
                                                                                                                        
                                                                                                                                sc.AddSingleton<ImageScanner>(sp => new ImageScanner(
                                                                                                                                            sp.GetRequiredService<IVectorService>(),
                                                                                                                                                        sp.GetRequiredService<IVectorDatabase>(),
                                                                                                                                                                    sp.GetRequiredService<StorageService>()));
                                                                                                                                                                    
                                                                                                                                                                            sc.AddSingleton<MainViewModel>();
                                                                                                                                                                            
                                                                                                                                                                                    Services = sc.BuildServiceProvider();
                                                                                                                                                                                    
                                                                                                                                                                                            // Apply saved theme
                                                                                                                                                                                                    var theme = Services.GetRequiredService<ThemeService>();
                                                                                                                                                                                                            theme.LoadPreference();
                                                                                                                                                                                                            
                                                                                                                                                                                                                    var mainWindow = new MainWindow
                                                                                                                                                                                                                            {
                                                                                                                                                                                                                                        DataContext = Services.GetRequiredService<MainViewModel>()
                                                                                                                                                                                                                                                };
                                                                                                                                                                                                                                                        mainWindow.Show();
                                                                                                                                                                                                                                                            }
                                                                                                                                                                                                                                                            }
