using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Avocado.ViewModel {
	public class MainViewModel {
		public static string Nickname { get; private set; } = "TestIRCNickname";
		public static string Realname { get; private set; } = "TestIRCRealname";

		public static ObservableCollection<ServerViewModel> Servers { get; set; }= new ObservableCollection<ServerViewModel>();

		public MainViewModel() {
			NewServer("irc.foonetic.net", 6667);
		}

		public void NewServer(string address, int port) {
			Servers.Add(new ServerViewModel(address, port));
		}
	}
}
