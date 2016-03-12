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

		private void CheckChangeHost(string hostname) {
			if (!GetHost(hostname).Equals(GetHost(Hostname))) return;

			GetChannel(Hostname).Name = hostname;
			Hostname = hostname;
		}

		private void CheckCreateChannel(string name) {
			if (!name.StartsWith("#") ||
				GetChannel(name) != null) return;

			CreateChannel(name);
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

		public void DisplayMessage(DisplayableMessage message) {
			if (message == null ||
				!message.DoDisplay ||
				string.IsNullOrEmpty(message.Message)) return;

			GetChannel(message.TargetTab).AppendMessage(message);
		}

		private void OnMessageRecieved(Message message) {
			if (message.IsPing) {
				Connection.Write(message.Args);
				return;
			}

			CheckCreateChannel(message.Target);
			CheckChangeHost(message.Target);

			Debug.WriteLine(message.RawMessage, "Output");
			DisplayMessage(ProcessMessage(message));
		}

		private void SendMessageOnEvent(object sender, Message message) {
			Connection.Write(message);
		}

		private void NotifyPropertyChanged(string info) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		public DisplayableMessage ProcessMessage(Message message) {
			switch (message.Type) {
				case "NOTICE":
					return new DisplayableMessage(Hostname, Hostname, message.Args);
				case "MODE":
					if (!IsVerified) IsVerified = !IsVerified;
					return new DisplayableMessage(message.Nickname,
						IrrelevantNicknames.Contains(message.Nickname) ? Hostname : message.Target, message.Args);
				case "PART":
					if (message.Target.StartsWith("#")) Channels.Remove(GetChannel(message.Target));
					break;
				case "353": // names reply
					message.Target = message.SplitArgs[1]; // SplitArgs[1] is the targeted channel in this instance
					ChannelViewModel channel = GetChannel(message.Target);

					foreach (string s in message.Args.Trim().Split(':')[1].Split(' ')) {
						channel.AddUser(s);
					}

					return new DisplayableMessage(message.Target, message.Target,
						string.Concat("Users in this channel: ", string.Join(", ", channel.Users.Select(user => user.Name).ToList())));
				case "366": // end of names reply
					// in this instance, SplitArgs[0] is the target channel
					SelectedChannel = GetChannel(message.SplitArgs[0]);
					break;
				default:
					return new DisplayableMessage(message);
			}

			return null;
		}
	}
}