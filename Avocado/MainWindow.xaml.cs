using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Avocado.IRC;

namespace Avocado {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public List<Server> Servers = new List<Server>();

		public MainWindow() {
			InitializeComponent();
		}

		public void AddTab(TabItem tab) {
			TabControlPrimary.Items.Add(tab);
		}

		public void SelectTab(int tabIndex) {
			TabControlPrimary.SelectedIndex = tabIndex;
		}

		private void ButtonConnect_Click(object sender, RoutedEventArgs e) {
			Thread serverThread = new Thread(() => {
				int index = Servers.Count == 0 ? Servers.Count : Servers.Count + 1;
				Servers.Add(new Server("irc.foonetic.net", "testbot", "testbot", 6667, index));

				DispatchAction(() => {
					SelectTab(index);
				});

				Servers.Last().RunServer();
			});
			serverThread.SetApartmentState(ApartmentState.STA);
			serverThread.Start();
		}

		public static void DispatchAction(Action action) {
			if (Application.Current.Dispatcher.CheckAccess()) {
				action?.Invoke();
			} else {
				Application.Current.Dispatcher?.BeginInvoke(DispatcherPriority.Normal, action);
			}
		}
	}
}
