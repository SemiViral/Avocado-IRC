using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;
using Avocado.Model.Messages;

namespace Avocado.Model {
    public class Connection : IDisposable {
        private bool _disposed;
        private StreamReader _reader;
        private NetworkStream _stream;

        private TcpClient _tcp;
        private StreamWriter _writer;

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

        public event EventHandler<ChannelMessage> MessageRecieved;

        protected virtual void Dispose(bool dispose) {
            if (!dispose || _disposed) return;

            _writer.Dispose();
            _reader.Dispose();
            _stream.Dispose();
            _tcp.Close();

            _disposed = true;
        }

        internal void Connect() {
            try {
                _tcp = new TcpClient(Address, Port);
                _stream = _tcp.GetStream();
                _writer = new StreamWriter(_stream);

                Debug.WriteLine($"Connection opened to address {Address}");
            } catch (SocketException) {
                IsInitiated = false;
                return;
            }

            Write(new OutputMessage("USER", $"{MainViewModel.Nickname} 0 * {MainViewModel.Realname}"));
            Write(new OutputMessage("NICK", MainViewModel.Nickname));

            IsInitiated = true;
        }

        internal void Listen() {
            using (_reader = new StreamReader(_stream)) {
                while (IsInitiated) {
                    string data = _reader.ReadLine();

                    if (string.IsNullOrEmpty(data)) {
                        //MessageRecieved?.Invoke(this, new ErrorMessage(Address, "Server disconnected."));
                        MessageBox.Show($"Server {Address} disconnected.");
                        IsInitiated = false;
                        continue;
                    }

                    MessageRecieved?.Invoke(this, new ChannelMessage(data));
                }
            }
        }

        public void Write(Message message) {
            Debug.WriteLine(message.RawMessage, "Input");
            _writer.WriteLine(message.RawMessage);
            _writer.Flush();
        }

        public void Write(string message) {
            Debug.WriteLine(message, "Input");
            _writer.WriteLine(message);
            _writer.Flush();
        }
    }
}