using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Avocado.IRC.Types;

namespace Avocado.IRC {
	public class Server {
		public string Address { get; }
		public string Nickname { get; }
		public string Realname { get; }
		public int Port { get; }
		public int Index { get; }

		private bool ServerIsIdentified { get; set; }
		private bool Initialised { get; set; }
		private bool ShouldRun { get; set; }

		private List<TabLayout> Tabs { get; } = new List<TabLayout>();

		private List<string> Channels { get; } = new List<string>();

		private TcpClient _tcp;
		private NetworkStream _stream;
		private StreamWriter _writer;
		private StreamReader _reader;

		public Server(string address, string nickname, string realname, int port, int index) {
			Address = address;
			Nickname = nickname;
			Realname = realname;
			Port = port;
			Index = index;

			CreateTab(Address);

			Initialised = true;
		}

		public void CreateTab(string header) {
			TabLayout tabLayout = new TabLayout(Index, header);
			TabItem serverTab = new TabItem {
				Header = header,
				Content = tabLayout
			};

			tabLayout.SendMessageEvent += ChannelSendEvent;

			Tabs.Add(tabLayout);

			MainWindow.DispatchAction(() => {
				((MainWindow)Application.Current.MainWindow).TabControlPrimary.Items.Add(serverTab);
			});
		}

		public void Connect() {
			WriteToTextbox(Address, $"Initialising connection to {Address}\r\n");
			_tcp = new TcpClient(Address, Port);
			_stream = _tcp.GetStream();
			_writer = new StreamWriter(_stream);
			_reader = new StreamReader(_stream);

			ShouldRun = true;

			Write("USER", $"{Nickname} 0 * {Realname}\r\n");
			Write("NICK", $"{Nickname}\r\n");
		}

		public void ConnectToChannel(string channel) {
			Write("JOIN", channel);

			CreateTab(channel);
		}

		public void RunServer() {
			Connect();

			ListenOnStream();
			//Thread listenThread = new Thread(ListenOnStream);
			//listenThread.SetApartmentState(ApartmentState.STA);
			//listenThread.Start();
		}

		private void ListenOnStream() {
			string data;

			while (!string.IsNullOrEmpty(data = _reader.ReadLine())) {
				ProcessMessage(data);
			}
		}

		public void Write(string type, string text, string recipient = null) {
			string writable = string.IsNullOrEmpty(recipient)
				? string.Concat(type, " ", recipient, " ", text) : string.Concat(type, " ", text);
				_writer.WriteLine(writable);
				_writer.Flush();
		}

		public void ProcessMessage(string text) {
			if (text.StartsWith("PING")) {
				Write("PONG", text.Substring(5));
				return;
			}

			ChannelMessage message = new ChannelMessage(text);

			// means channel user list
			if (CheckMessageType(message)) return;

			CheckCreateTab(message);

			WriteToTextbox(message);
		}

		private bool CheckMessageType(ChannelMessage message) {
			switch (message.Type) {
				case "JOIN":
					if (!message.Nickname.Equals(Nickname)) break;

					CreateTab(message.Recipient);
					return true;
				case "353":
					foreach (string nickname in message.Args.Split(':')[1].Split(' ')) {
						Tabs.First(tab => tab.TabName.Equals(message.Recipient)).AddUserToList(new ChannelUser(nickname));
					}
					return true;
			}

			return false;
		}

		private void CheckCreateTab(ChannelMessage message) {
			if (Tabs.FirstOrDefault(tab => tab.TabName.Equals(message.Nickname)) == null) {
				CreateTab(message.Nickname);
			}
		}

		private void WriteToTextbox(ChannelMessage message) {
			if (message.IsRealUser && !message.Nickname.Equals(Nickname))
				Tabs.First(tab => tab.TabName.Equals(message.Recipient))
					.Output($"[{DateTime.Now.ToString("hh:mm:ss")} {message.Recipient}] {message.Nickname}: {message.Args}");
			else
				Tabs.First(tab => tab.TabName.Equals(Address))
					.Output($"[{DateTime.Now.ToString("hh:mm:ss")}] {message.Nickname}: {message.Args}");
		}

		private void WriteToTextbox(string recipient, string message) {
			Tabs.First(tab => tab.TabName.Equals(recipient)).Output(message);
		}

		public void ChannelSendEvent(object sender, MessageEventArgs e) {
			Write(e.Type, e.Message, e.Target);
		}
	}

	public class MessageEventArgs {
		public string Type { get; }
		public string Target { get; }
		public string Message { get; }

		public MessageEventArgs(string type, string target, string message) {
			Type = type;
			Target = target;
			Message = message;
		}
	}
}
