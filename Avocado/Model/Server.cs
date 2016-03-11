using System;
using System.IO;
using System.Net.Sockets;

namespace Avocado.Model {
	public class Server : IDisposable {
		private readonly TcpClient _tcp;
		private readonly NetworkStream _stream;
		private readonly StreamWriter _writer;
		private readonly StreamReader _reader;

		private bool _disposed;

		public Server(string address, int port) {
			_tcp = new TcpClient(address, port);
			_stream = _tcp.GetStream();
			_writer = new StreamWriter(_stream);
			_reader = new StreamReader(_stream);
		}

		public string Listen() {
			return _reader.ReadLine();
		}

		public void Write(Message message) {
			_writer.WriteLine(message.RawMessage);
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
