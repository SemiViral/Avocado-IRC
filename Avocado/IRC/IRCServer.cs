using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avocado.IRC.Types;

namespace Avocado.IRC {
	public class IrcServer {
		public string Hostname { get; private set; }
		public string Nickname { get; }
		public string Realname { get; }
		public int Index { get; }

		public IRCConnection Connection { get; }

		public Dictionary<string, Guid> TabReference = new Dictionary<string, Guid>(); 
		
		private bool Initialised { get; set; }
		private bool ShouldRun { get; set; }

		public IrcServer(string address, string nickname, string realname, int port, int index) {
			Hostname = address;
			Nickname = nickname;
			Realname = realname;
			Index = index;

			Connection = new IRCConnection(address, port);

			Connection.Write("USER", $"{nickname} 0 * {realname}\r\n");
			Connection.Write("NICK", $"{nickname}\r\n");

			ShouldRun = true;
		}

		//todo public void ConnectToChannel(string channel) {
		//	Connection.Write("JOIN", channel);

		//	CreateTab(channel);
		//}

		public void RunServer() {
			string data;

			while (!string.IsNullOrEmpty(data = Connection.Listen())) {
				ProcessMessage(data);
			}
		}

		public void ProcessMessage(string text) {
			if (text.StartsWith("PING")) {
				Connection.Write("PONG", text.Substring(5));
				return;
			}

			ChannelMessage message = new ChannelMessage(text);

			CheckCreateTab(message);

			// means channel user list
			if (CheckMessageType(message)) return;

			WriteToTextbox(message);
		}

		private void CheckCreateTab(ChannelMessage message) {
			if (MainWindow.CheckTabExists(message.Target)) return;

			if (!Initialised && !message.IsRealUser)
				if (GetBaseHostname(message.Target).Equals(GetBaseHostname(Hostname))) {
					Hostname = message.Target;

					MainWindow.CreateTab(Index, Hostname);
					Initialised = true;
				}

			switch (message.Type) {
				case "JOIN":
					if (!message.Nickname.Equals(Nickname)) break;

					MainWindow.CreateTab(Index, message.Target);
					break;
			}
		}

		public static string GetBaseHostname(string hostname) {
			Debug.WriteLine($"||| {hostname.Substring(hostname.IndexOf(".", StringComparison.Ordinal) + 1)} |||");
			return hostname.Substring(hostname.IndexOf(".", StringComparison.Ordinal) + 1);
		}

		private bool CheckMessageType(ChannelMessage message) {
			switch (message.Type) {
				case "JOIN":
					if (!message.Nickname.Equals(Nickname)) break;

					//todo CreateTab(message.Target);
					return true;
				//case "353":
				//	foreach (string nickname in message.Args.Split(':')[1].Split(' ')) {
				//		Tabs.First(tab => tab.TabName.Equals(message.Target)).AddUserToList(new ChannelUser(nickname));
				//	}
				//	return true;
			}

			return false;
		}

		private void WriteToTextbox(ChannelMessage message) {
			if (message.Nickname.Equals(Nickname)) return;

			MainWindow.GetLayout(Index, message.Target)
				.Output($"[{DateTime.Now.ToString("hh:mm:ss")}] {message.Target}: {message.Args}");
		}

		private void WriteToTextbox(string recipient, string message) {
			MainWindow.GetLayout(Index, recipient).Output(message);
		}

		public void ChannelSendEvent(object sender, MessageEventArgs e) {
			Connection.Write(e.Type, e.Message, e.Target);
		}
	}

	public class MessageEventArgs : EventArgs {
		public string Type { get; }
		public string Target { get; }
		public string Message { get; }

		public MessageEventArgs(string type, string target, string message) {
			Type = type;
			Target = target;
			Message = message;
		}
	}

	public static class ServerExtension {
		public static void AddTab(this IrcServer obj, string tabName, Guid uniqueId) {
			obj.TabReference.Add(tabName, uniqueId);
		}
	}
}
