using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Windows;

namespace Avocado.Model {
	public class Server : IDisposable {
		private TcpClient _tcp;
		private NetworkStream _stream;
		private StreamWriter _writer;
		private StreamReader _reader;

		private bool _disposed;

		public bool ShouldListen { private get; set; }

		public event EventHandler<Message> MessageRecieved;

		public Server(string address, int port) {
			Connect(address, port);

			ShouldListen = true;
		}

		public void Connect(string address, int port) {
			try {
				_tcp = new TcpClient(address, port);
				_stream = _tcp.GetStream();
				_writer = new StreamWriter(_stream);
				_reader = new StreamReader(_stream);

				Debug.WriteLine($"Connection opened to address {address}");
			} catch (Exception e) {
				MessageBox.Show($"Error occured connecting to server: {e}");
			}
		}

		public void Listen() {
			while (ShouldListen) {
				string data = _reader.ReadLine();

				if (string.IsNullOrEmpty(data)) {
					MessageBox.Show("Server disconnected.");
					ShouldListen = false;
					return;
				}
				Debug.WriteLine(data);
				MessageRecieved?.Invoke(this, new Message(data));
			}
		}

		public void Write(Message message) {
			Debug.WriteLine(message.RawMessage);
			_writer.WriteLine(message.RawMessage);
			_writer.Flush();
		}

		public void Write(string message) {
			Debug.WriteLine(message);
			_writer.WriteLineAsync(message);
			_writer.Flush();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool dispose) {
			if (!dispose || _disposed) return;
			
			_writer.Dispose();
			_reader.Dispose();
			_stream.Dispose();
			_tcp.Close();

			_disposed = true;
		}
	}
}
