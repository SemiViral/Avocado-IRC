using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using Avocado.Model;

namespace Avocado.ViewModel {
	public class ServerViewModel {
		public ServerViewModel(string address, int port) {
			Hostname = address;

			Connection = new Server(address, port);

			ShouldRun = true;
		}

		public bool ShouldRun { get; private set; }
		public string Hostname { get; private set; }

		public Server Connection { get; }
		public ChannelViewModel SelectedChannel { get; set; }

		public ObservableCollection<ChannelViewModel> Channels { get; }= new ObservableCollection<ChannelViewModel>();
		public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
		public List<Message> TempraryArchive = new List<Message>();

		public void CreateChannel(ChannelViewModel channel) {
			channel.SendMessageEvent += SendMessageOnEvent;
			Channels.Add(channel);
		}

		public void SendMessageOnEvent(object sender, Message message) {
			Connection.Write(message);
		}

		public static string GetHost(string hostname) {
			return hostname.Substring(hostname.IndexOf(".", StringComparison.Ordinal) + 1);
		}

		public static void DispatchAction(Action action) {
			if (Application.Current.Dispatcher.CheckAccess()) {
				action?.Invoke();
			} else {
				Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
			}
		}
	}
}
