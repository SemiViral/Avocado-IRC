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

		private string _hostname;

		private ChannelViewModel _selectedChannel;

		public ServerViewModel(string address, int port) {
			Hostname = address;
			Port = port;

			CreateChannel(Hostname);

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

		private bool IsVerified { get; set; }

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

		public ChannelViewModel SelectedChannel {
			get { return _selectedChannel; }
			set {
				if (value == _selectedChannel) return;

				_selectedChannel = value;
				NotifyPropertyChanged("SelectedChannel");
				Debug.WriteLine($"Selected channel changed to {value.Name}", "Information");
			}
		}

		public ObservableCollection<ChannelViewModel> Channels { get; } = new ObservableCollection<ChannelViewModel>();

		public ObservableCollection<ChannelViewModel> ShortChannels {
			get { return new ObservableCollection<ChannelViewModel>(Channels.Where(chan => chan.Name != Hostname)); }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public void PreConnect() {
			Connection.Write(new OutputMessage("USER", $"{MainViewModel.Nickname} 0 * {MainViewModel.Realname}"));
			Connection.Write(new OutputMessage("NICK", MainViewModel.Nickname));
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

		private void CheckChangeHost(string hostname) {
			if (!GetHost(hostname).Equals(GetHost(Hostname))) return;

			GetChannel(Hostname).Name = hostname;
			Connection.Address = hostname;
			Hostname = hostname;
		}

		private void CheckCreateChannel(string name) {
			if (!name.StartsWith("#") ||
				GetChannel(name) != null) return;

			CreateChannel(name);
		}

		// this was a grand waste of time...
		//public bool IsIrrelevantMessage(Message Args) {
		//	if (Args.Target.Equals(MainViewModel.Nickname)) {
		//		Args.Target = Hostname;
		//		return true;
		//	}

		//	if (Args.IsRealUser) return false;
		//	if (string.IsNullOrEmpty(Args.Nickname) ||
		//		!IrrelevantNicknames.Contains(Args.Nickname)) return false;
		//	if (!string.IsNullOrEmpty(Args.Target) &&
		//		Channels.Any(chan => IrrelevantNicknames.Contains(chan.Name))) return false;

		//	Args.Target = Hostname;
		//	return true;
		//}

		public static void DispatchAction(Action action) {
			if (Application.Current == null) return;

			if (Application.Current.Dispatcher.CheckAccess()) action?.Invoke();
			else Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
		}

		public void DisplayMessage(Message message) {
			if (string.IsNullOrEmpty(message?.Args)) return;

			GetChannel(message.Target).AppendMessage(message);
		}

		private void OnMessageRecieved(ChannelMessage message) {
			if (message.IsPing) {
				Connection.Write(message.Args);
				return;
			}

			CheckCreateChannel(message.Target);
			CheckChangeHost(message.Target);

			Debug.WriteLine(message.RawMessage, "Output");
			DisplayMessage(ProcessMessage(message));
		}

		private void SendMessageOnEvent(object sender, OutputMessage message) {
			Connection.Write(message);
		}

		private void NotifyPropertyChanged(string info) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		public Message ProcessMessage(ChannelMessage message) {
			switch (message.Type) {
				case "NOTICE":
					return new Message(Hostname, Hostname, message.Args);
				case "MODE":
					if (!IsVerified) IsVerified = !IsVerified;
					return new Message(message.Nickname,
						IrrelevantNicknames.Contains(message.Nickname) ? Hostname : message.Target, message.Args);
				case "PART":
					if (message.Target.StartsWith("#") && GetChannel(message.Target) != null) Channels.Remove(GetChannel(message.Target));
					break;
				case "353": // names reply
					message.Target = message.SplitArgs[1]; // SplitArgs[1] is the targeted channel in this instance
					ChannelViewModel channel = GetChannel(message.Target);

					foreach (string s in message.Args.Trim().Split(':')[1].Split(' ')) {
						channel.AddUser(s);
					}

					return new Message(message.Target, message.Target,
						string.Concat("Users in this channel: ", string.Join(", ", channel.Users.Select(user => user.Name).ToList())));
				case "366": // end of names reply
					// in this instance, SplitArgs[0] is the target channel
					SelectedChannel = GetChannel(message.SplitArgs[0]);
					break;
				default:
					return new Message(message.Nickname, message.Target, message.Args);
			}

			return null;
		}
	}
}