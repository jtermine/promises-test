using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestPromiseConsole
{
    internal static class Client
    {
        private const int SendChunkSize = 256;
        private const int ReceiveChunkSize = 256;
        private const bool Verbose = true;
        private static readonly object ConsoleLock = new object();
        private static readonly TimeSpan Delay = TimeSpan.FromMilliseconds(30000);
        private static readonly UTF8Encoding Encoder = new UTF8Encoding();
        private static ClientWebSocket _webSocket;

        private static void Main(string[] args)
        {
            Thread.Sleep(1000);
            Connect("ws://localhost.fiddler:2950/echo").Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task Connect(string uri)
        {
            try
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
                await Task.WhenAll(Receive(), Send());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex);
            }
            finally
            {
                if (_webSocket != null)
                    _webSocket.Dispose();
                Console.WriteLine();

                lock (ConsoleLock)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("WebSocket closed.");
                    Console.ResetColor();
                }
            }
        }

        private static async Task Send()
        {
            //byte[] buffer = encoder.GetBytes("{\"op\":\"blocks_sub\"}"); //"{\"op\":\"unconfirmed_sub\"}");
            byte[] buffer = Encoder.GetBytes("{\"op\":\"unconfirmed_sub\"}");

            for (int i = 0; i < 20; i++)
            {
                await
                    _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true,
                        CancellationToken.None);

                while (_webSocket.State == WebSocketState.Open)
                {
                    LogStatus(false, buffer, buffer.Length);
                    await Task.Delay(Delay);
                }    
            }
            
        }

        private static async Task Receive()
        {
            var buffer = new byte[ReceiveChunkSize];
            while (_webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result =
                    await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    LogStatus(true, buffer, result.Count);
                }
            }
        }

        private static void LogStatus(bool receiving, byte[] buffer, int length)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = receiving ? ConsoleColor.Green : ConsoleColor.Gray;
                //Console.WriteLine("{0} ", receiving ? "Received" : "Sent");

                Console.WriteLine(Encoder.GetString(buffer));

                Console.ResetColor();
            }
        }
    }
}