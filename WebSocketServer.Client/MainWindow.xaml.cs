using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace WebSocketServer.Client
{
    public partial class MainWindow : Window
    {
        private ClientWebSocket socket;

        public MainWindow()
        {
            InitializeComponent();
            socket = new ClientWebSocket();
        }
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await socket.ConnectAsync(new Uri("ws://localhost:3000"), CancellationToken.None);
                AppendToStatus("Connected to server");
                Listen();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void Listen()
        {
            var buffer = new byte[1024];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    AppendToOutput($"Server says: {message}");
                }
            }
        }
        private void AppendToStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(message));
                StatusText.Document.Blocks.Add(paragraph);
            });
        }
        private void AppendToOutput(string message)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(message));
                OutputText.Document.Blocks.Add(paragraph);
            });
        }
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = UsernameText.Text;
                string email = EmailText.Text;
                string password = PasswordText.Text;

                var messageType = "caca"; // ignore this luv <3
                var payload = new
                {
                    type = messageType,
                    data = new
                    {
                        password = password,
                        email = email,
                        username = username
                    }
                };
                var jsonMessage = JsonConvert.SerializeObject(payload);

                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(jsonMessage));
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

                Console.WriteLine($"Sent to server: {jsonMessage}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (socket.State == WebSocketState.Open)
            {
                socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None).Wait();
            }
        }
    }
}
