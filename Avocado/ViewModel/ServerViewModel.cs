using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Avocado.Model;

namespace Avocado.ViewModel {
	public class ServerViewModel {
		public ServerViewModel(string address, int port) {
			Hostname = address;
			Port = port;

			Connection = new Server(address, port);
			Connection.MessageRecieved += OutputMessageOnEvent;

			Thread connectionThread = new Thread(Connection.Listen);
			connectionThread.Start();

			PreConnect();

			CreateChannel(Hostname);
			CreateChannel("Jesuits");
			ShouldRun = true;

		}

		public bool ShouldRun { get; private set; }
		public bool IsVerified { get; private set; }

		private static readonly string[] IrrelevantNicknames = {"NickServ", "ChanServ"};

		public string Hostname { get; set; }

		public int Port { get; private set; }

		public Server Connection { get; }
		public ChannelViewModel SelectedChannel { get; set; }

		private ListBoxItem _selectedItem;
		public ListBoxItem SelectedItem {
			get {
				return _selectedItem;
			}
			set {
				_selectedItem = value;

				Debug.WriteLine(value.Content.ToString());
				SelectedChannel = GetChannel(value.Name);
			}
		}

		public ObservableCollection<ChannelViewModel> Channels { get; } = new ObservableCollection<ChannelViewModel>();

		public ObservableCollection<ChannelViewModel> ShortChannels {
			get { return new ObservableCollection<ChannelViewModel>(Channels.Where(chan => chan.Name != Hostname)); }
		}

		public void PreConnect() {
			Connection.Write(new Message("USER", $"{MainViewModel.Nickname} 0 * {MainViewModel.Realname}"));
			Connection.Write(new Message("NICK", MainViewModel.Nickname));
		}

		public void CreateChannel(string name) {
			ChannelViewModel temp = new ChannelViewModel(name);
			temp.SendMessageEvent += SendMessageOnEvent;
			Channels.Add(temp);
		}

		public static string GetHost(string hostname) {
			return string.IsNullOrEmpty(hostname) ? null : hostname.Substring(hostname.IndexOf(".", StringComparison.Ordinal) + 1);
		}

		public ChannelViewModel GetChannel(string name) {
			return Channels.FirstOrDefault(channel => channel.Name.Equals(name));
		}

		public void CheckChangeHost(string hostname) {
			if (string.IsNullOrEmpty(hostname)
				|| !GetHost(hostname).Equals(GetHost(Hostname))) return;
			
			foreach (ChannelViewModel channelViewModel in Channels) {
				Debug.WriteLine(channelViewModel.Name);
			}
			GetChannel(Hostname).Name = hostname;
			Hostname = hostname;
		}

		public bool IsIrrelevantMessage(Message message) {
			if (!IsVerified || !IrrelevantNicknames.Contains(message.Nickname)) return false;

			return !Channels.Any(chan => IrrelevantNicknames.Contains(chan.Name));
		}

		public static void DispatchAction(Action action) {
			if (Application.Current.Dispatcher.CheckAccess()) {
				action?.Invoke();
			} else {
				Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
			}
		}

		public void OnSelectionChangedEvent(object sender, SelectionChangedEventArgs e) {
			foreach (var item in e.AddedItems) {
				Debug.WriteLine(item.GetType() + item.ToString());
			}
		}

		private void OutputMessageOnEvent(object sender, Message message) {
			if (message.IsPing) {
				Connection.Write(message.Args);
				return;
			}

			if (!message.IsRealUser
				|| IsIrrelevantMessage(message)) {
				CheckChangeHost(message.Target);
				message.Target = Hostname;
			}
			
			Debug.WriteLine(message.RawMessage + " " + message.Target);

			DispatchAction(() => GetChannel(message.Target).AppendMessage(message));
		}

		private void SendMessageOnEvent(object sender, Message message) {
			Connection.Write(message);
		}
	}
}
