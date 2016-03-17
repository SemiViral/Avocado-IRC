using System.Collections.ObjectModel;

namespace Avocado.ViewModel {
    public class MainViewModel {
        public MainViewModel() {
            NewServer("irc.foonetic.net", 6667);
        }

        public static string Nickname { get; private set; } = "TestIRCNickname";
        public static string Realname { get; private set; } = "TestIRCRealname";

        public static ObservableCollection<ServerViewModel> Servers { get; set; } =
            new ObservableCollection<ServerViewModel>();

        public void NewServer(string address, int port) {
            Servers.Add(new ServerViewModel(address, port));
        }
    }
}