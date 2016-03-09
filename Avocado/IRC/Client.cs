using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Avocado.IRC {
	public class Client : IDisposable {
		public string Nickname = "TestNickname",
			Realname = "TestRealname";

		// for testing only
		public string Address = "irc.foonetic.net",
			Channel = "#testgrounds";

		public int Port = 6667;

		private bool _disposed;
		
		private List<Socket> _connections = new List<Socket>();

		/// <summary>
		///     Dispose of all streams and objects
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool dispose) {
			if (!dispose || _disposed) return;

			foreach (Socket socket in _connections) {
				socket.Close();
			}

			_disposed = true;
		}
	}
}
