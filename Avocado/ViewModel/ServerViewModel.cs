using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Avocado.Model;

namespace Avocado.ViewModel {
	public class ServerViewModel : INotifyPropertyChanged {
		private static readonly string[] IrrelevantNicknames = {"NickServ", "ChanServ", MainViewModel.Nickname};

		private bool _hostIsSelected;

		public ServerViewModel(string address, int port) {
			Hostname = address;
			Port = port;

			CreateChannel(Hostname);
			CreateChannel("Jesuits");
			CreateChannel("Jesuitsv2");

			Connection = new Server(address, port);
			Connection.MessageRecieved += (sender, message) => DispatchAction(() => OnMessageRecieved(message));

			Thread connectionThread = new Thread(Connection.Listen) {
				IsBackground = true
			};
			connectionThread.Start();

			PreConnect();

			ShouldRun = true;
		}

		public bool ShouldRun { get; private set; }

		public bool HostIsSelected {
			get { return _hostIsSelected; }
			set {
				_hostIsSelected = value;
				Debug.WriteLine(value);
				if (!value) return;

				SelectedChannel = GetChannel(Hostname);
			}
		}

		public bool IsVerified { get; } = false;

		private string _hostname;
		public string Hostname {
			get { return _hostname; }
			set {
				if (value == _hostname) return;

				_hostname = value;
				NotifyPropertyChanged("Hostname");
			}
		}

		public int Port { get; private set; }

		public Server Connection { get; }
		public ChannelViewModel SelectedChannel { get; set; }

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
			return string.IsNullOrEmpty(name) ? null : Channels.FirstOrDefault(channel => channel.Name.Equals(name));
		}

		public void CheckChangeHost(string hostname) {
			if (!GetHost(hostname).Equals(GetHost(Hostname))) return;

			GetChannel(Hostname).Name = hostname;
			Hostname = hostname;
		}

		// this was a grand waste of time...
		//public bool IsIrrelevantMessage(Message message) {
		//	if (message.Target.Equals(MainViewModel.Nickname)) {
		//		message.Target = Hostname;
		//		return true;
		//	}

		//	if (message.IsRealUser) return false;
		//	if (string.IsNullOrEmpty(message.Nickname) ||
		//		!IrrelevantNicknames.Contains(message.Nickname)) return false;
		//	if (!string.IsNullOrEmpty(message.Target) &&
		//		Channels.Any(chan => IrrelevantNicknames.Contains(chan.Name))) return false;

		//	message.Target = Hostname;
		//	return true;
		//}

		public static void DispatchAction(Action action) {
			if (Application.Current == null) return;

			if (Application.Current.Dispatcher.CheckAccess()) action?.Invoke();
			else Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
		}

		private void OnMessageRecieved(Message message) {
			if (message.IsPing) {
				Connection.Write(message.Args);
				return;
			}

			if (message.Type.Equals("NOTICE") ||
				message.Type.Equals("MODE"))
				message.Target = Hostname;

			CheckChangeHost(message.Target);

			Debug.WriteLine(message.RawMessage);
			GetChannel(message.Target).AppendMessage(message);
		}

		private void SendMessageOnEvent(object sender, Message message) {
			Connection.Write(message);
		}

		private void NotifyPropertyChanged(string info) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}