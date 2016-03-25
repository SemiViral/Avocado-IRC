using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using Avocado.Model.Messages;

namespace Avocado.Model {
    public class Connection : IDisposable {
        private bool disposed;
        private StreamReader reader;
        private NetworkStream stream;

        private TcpClient tcp;
        private StreamWriter writer;

        public Connection(string address, int port) {
            Address = address;
            Port = port;
        }

        public bool IsInitiated { get; private set; }

        public string Address { get; set; }
        public int Port { get; set; }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public event MessageEventHandler MessageRecieved;

        protected virtual void Dispose(bool dispose) {
            if (!dispose || disposed) return;

            writer.Dispose();
            reader.Dispose();
            stream.Dispose();
            tcp.Close();

            disposed = true;
        }

        /// <summary>
        ///     Initialises connection
        /// </summary>
        internal void Connect() {
            try {
                tcp = new TcpClient(Address, Port);
                stream = tcp.GetStream();
                writer = new StreamWriter(stream);

                Debug.WriteLine($"Connection opened to address {Address}");
            } catch (SocketException) {
                IsInitiated = false;
                return;
            }

            IsInitiated = true;
        }

        /// <summary>
        /// Initial identification of client with server
        /// </summary>
        internal void Identify() {
            Write(new OutputMessage("USER", $"{MainViewModel.Nickname} 0 * {MainViewModel.Realname}"));
            Write(new OutputMessage("NICK", MainViewModel.Nickname));
        }
        
        /// <summary>
        /// Listens infinitely on connection socket
        /// </summary>
        internal void Listen() {
            using (reader = new StreamReader(stream)) {
                while (IsInitiated) {
                    string data = reader.ReadLine();

                    if (string.IsNullOrEmpty(data)) {
                        MessageRecieved?.Invoke(this, new ErrorMessage(Address, "Server disconnected."));
                        MessageBox.Show($"Server {Address} disconnected.");
                        IsInitiated = false;
                        continue;
                    }

                    MessageRecieved?.Invoke(this, new ChannelMessage(data));
                }
            }
        }

        /// <summary>
        /// Writes to stream using message object
        /// </summary>
        /// <param name="message"></param>
        public void Write(Message message) {
            Debug.WriteLine(message.RawMessage, "Input");
            writer.WriteLine(message.RawMessage);
            writer.Flush();
        }

        /// <summary>
        /// Writes to stream with string
        /// </summary>
        /// <param name="message"></param>
        public void Write(string message) {
            Debug.WriteLine(message, "Input");
            writer.WriteLine(message);
            writer.Flush();
        }
    }
}