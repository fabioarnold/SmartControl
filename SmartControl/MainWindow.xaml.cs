using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TimeSpan _timeout = TimeSpan.FromHours(1); // Long polling

        private string Id
        {
            get => Properties.Settings.Default.ClientID;
            set
            {
                Properties.Settings.Default.ClientID = value;
                Properties.Settings.Default.Save();
            }
        }
        
        private ulong _offset;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            if (string.IsNullOrWhiteSpace(Id))
            {
                Id = Guid.NewGuid().ToString();
            }
            Title = $"{Environment.MachineName} ({Id})";
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            await PollCommands();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
        }

        private async Task PollCommands()
        {
            using (var client = new HttpClient() { BaseAddress = new Uri(TextBox_Url.Text), Timeout = _timeout })
            {
                while (!_cts.IsCancellationRequested)
                {
                    try
                    {
                        var name = Uri.EscapeDataString(Environment.MachineName);
                        var url = $"api/updates/{Id}/{name}/{_offset}";
                        Log($"Sending GET request to {url}");
                        var response = await client.GetAsync(url, _cts.Token);
                        var content = await response.Content.ReadAsStringAsync(_cts.Token);
                        Log($"Received response: {content}");
                        var updateResponse = JsonConvert.DeserializeObject<UpdateResponse>(content);
                        _offset = updateResponse.Offset;
                        HandleCommand(updateResponse.State);
                    }
                    catch (Exception ex)
                    {
                        Log(ex.Message);
                        await Task.Delay(1000);
                    }
                }
            }
        }

        private void HandleCommand(string command)
        {
            Grid_Main.Background = typeof(Brushes).GetProperty(command)?.GetValue(null) as Brush;
        }

        private void Log(string message)
        {
            TextBox_Log.Text += $"{DateTime.Now:u} {message} {Environment.NewLine}";
            TextBox_Log.ScrollToEnd();
        }
    }
}
