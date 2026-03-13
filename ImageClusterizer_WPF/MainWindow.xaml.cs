using System.Collections.Specialized;
using System.Windows;
using ImageClusterizer_WPF.ViewModels;

namespace ImageClusterizer_WPF;

public partial class MainWindow : Window
{
    public MainWindow()
        {
                InitializeComponent();
                    }

                        protected override void OnContentRendered(EventArgs e)
                            {
                                    base.OnContentRendered(e);
                                            if (DataContext is MainViewModel vm)
                                                    {
                                                                // Auto-scroll console to bottom on new lines
                                                                            vm.ConsoleLines.CollectionChanged += ConsoleLines_CollectionChanged;
                                                                                    }
                                                                                        }

                                                                                            private void ConsoleLines_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
                                                                                                {
                                                                                                        if (e.Action == NotifyCollectionChangedAction.Add)
                                                                                                                    ConsoleScrollViewer.ScrollToBottom();
                                                                                                                        }
                                                                                                                        
                                                                                                                            private void ClearConsole_Click(object sender, RoutedEventArgs e)
                                                                                                                                {
                                                                                                                                        if (DataContext is MainViewModel vm)
                                                                                                                                                    vm.ConsoleLines.Clear();
                                                                                                                                                        }
                                                                                                                                                        }
