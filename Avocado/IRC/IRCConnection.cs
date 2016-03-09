using System;
using System.IO;
using System.Net.Sockets;

namespace Avocado.IRC {
	public class IRCConnection : IDisposable {
		private readonly TcpClient _tcp;
		private readonly NetworkStream _stream;
		private readonly StreamWriter _writer;
		private readonly StreamReader _reader;

		private bool _disposed;

		public IRCConnection(string address, int port) {
			_tcp = new TcpClient(address, port);
			_stream = _tcp.GetStream();
			_writer = new StreamWriter(_stream);
			_reader = new StreamReader(_stream);
		}

		public string Listen() {
			return _reader.ReadLine();
		}

		public void Write(string type, string text, string recipient = null) {
			string writable = string.IsNullOrEmpty(recipient)
				? string.Concat(type, " ", recipient, " ", text) : string.Concat(type, " ", text);
			_writer.WriteLine(writable);
			_writer.Flush();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool dispose) {
			if (!dispose || _disposed) return;
			
			_writer?.Dispose();
			_reader?.Dispose();
			_stream?.Dispose();
			_tcp.Close();

			_disposed = true;
		}
	}
}
