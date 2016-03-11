using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Avocado.ViewModel {
	public class MainViewModel {
		public static string Nickname { get; private set; } = "TestIRCNickname";
		public static string Realname { get; private set; } = "TestIRCRealname";

		public static ObservableCollection<ServerViewModel> Servers { get; set; }= new ObservableCollection<ServerViewModel>();

		public void NewServer(string address, int port) {
			try {
				Servers.Add(new ServerViewModel(address, port));
			} catch (Exception) {
				MessageBox.Show("Error occured connecting to server!");
			}
		}
	}
}
