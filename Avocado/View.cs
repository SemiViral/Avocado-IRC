using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Avocado.IRC;
using Avocado.IRC.Types;

namespace Avocado {
	public class View {
		public static List<IrcServer> Servers = new List<IrcServer>();
		public static List<TabLayout> Tabs = new List<TabLayout>();
		private static ObservableCollection<ChannelMessage> OutputStack { get; } = new ObservableCollection<ChannelMessage>();

		public static void Initialise() {
			OutputStack.CollectionChanged += OnStackAdded;
		}

		public static void AppendMessageToOutputStack(ChannelMessage message) {
			OutputStack.Add(message);
		}

		public static void OnStackAdded(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action != NotifyCollectionChangedAction.Add) return;

			foreach (object r in e.NewItems) {
				
			}
		}

		public static IrcServer GetServer(int serverIndex) {
			return Servers.FirstOrDefault(serverObj => serverObj.Index == serverIndex);
		}

		public static TabLayout GetLayout(int serverIndex, string tabName) {
			//IrcServer serverObj = Servers.FirstOrDefault(index => index.TabReference.ContainsKey(tabName));
			//return GetServer(serverIndex)?.TabReference
			//		.Any(item => item.Key.Equals(tabName)) == false ? null
			//		: Tabs.FirstOrDefault(tab => tab.UniqueId == serverObj?.TabReference.First(kvp => kvp.Key.Equals(tabName)).Value);
			return Tabs.FirstOrDefault(tab => tab.ServerIndex == serverIndex && tab.TabName.Equals(tabName));
		}

		public static void CreateTab(int serverIndex, string tabName) {
			DispatchAction(() => {
				WriteCurrentThreadId(1);

				IrcServer temp = GetServer(serverIndex);
				if (temp == null) return;

				TabLayout tabLayout = new TabLayout(serverIndex, tabName);
				TabItem tabItem = new TabItem {
					Header = tabName,
					Content = tabLayout
				};

				tabLayout.SendMessageEvent += temp.ChannelSendEvent;

				((MainWindow)Application.Current.MainWindow).AddTab(tabItem);
				Tabs.Add(tabLayout);

				GetServer(serverIndex).AddTab(tabLayout.TabName, tabLayout.UniqueId);
			});
		}

		public static void WriteCurrentThreadId(int customIdentitiyNumber) {
			Debug.WriteLine($"Custom ID for output: {customIdentitiyNumber}; Thread ID: {Thread.CurrentThread.ManagedThreadId}");
		}

		public static bool CheckTabExists(string name) {
			return Tabs.Any(tab => tab.TabName.Equals(name));
		}

		public static void DispatchAction(Action action) {
			if (Application.Current.Dispatcher.CheckAccess()) {
				action?.Invoke();
			} else {
				Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
			}
		}

		private void ButtonConnect_Click(object sender, RoutedEventArgs e) {
			Thread serverThread = new Thread(() => {
				int index = Servers.Count == 0 ? Servers.Count : Servers.Count + 1;
				Servers.Add(new IrcServer("irc.foonetic.net", "testbot", "testbot", 6667, index));

				DispatchAction(() => {
					SelectTab(index);
				});

				Servers.Last().RunServer();
			});

			serverThread.SetApartmentState(ApartmentState.STA);
			serverThread.Start();
		}



		public void AddTab(TabItem tab) {
			MainWindow.TabControlPrimary.Items.Add(tab);
		}

		public void SelectTab(int tabIndex) {
			MainWindow.TabControlPrimary.SelectedIndex = tabIndex;
		}
	}
}
