using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avocado.Model;

namespace Avocado {
    public class MainViewModel : INotifyPropertyChanged {
        private Channel _selectedChannel;
        private string _selectedChannelName;

        private string _serverAddress;
        private string _serverPort;

        public static string Nickname { get; private set; } = "TestIRCNickname";
        public static string Realname { get; private set; } = "TestIRCRealname";

        public string ServerAddress {
            get { return _serverAddress; }
            set {
                if (value.Equals(_serverAddress)) return;

                _serverAddress = value;
                NotifyPropertyChanged("ServerAddress");
            }
        }

        public string ServerPort {
            get { return _serverPort; }
            set {
                int temp;

                if (!int.TryParse(value, out temp) &&
                    !string.IsNullOrEmpty(value)) return;

                _serverPort = value;
                NotifyPropertyChanged("ServerPort");
            }
        }

        public ObservableCollection<Server> Servers { get; } =
            new ObservableCollection<Server>();

        public ObservableCollection<string> ChannelNames { get; } = new ObservableCollection<string>();

        public string SelectedChannelName {
            get { return _selectedChannelName; }
            set {
                if (string.IsNullOrEmpty(value) ||
                    value.Equals(_selectedChannelName)) return;

                _selectedChannelName = value;
                SetSelectedChannel(value);
                NotifyPropertyChanged("SelectedChannelName");
            }
        }

        public ICommand NewServerButtonCommand => new ActionCommand(NewServerButtonClick);

        public Channel SelectedChannel {
            get { return _selectedChannel; }
            set {
                if (value == _selectedChannel) return;

                _selectedChannel = value;
                NotifyPropertyChanged("SelectedChannel");
                Debug.WriteLine($"Selected channel changed to {value.Name}", "Information");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetSelectedChannel(string channelName) {
            Channel temp =
                Servers.SelectMany(server => server.Channels).FirstOrDefault(channel => channel.Name.Equals(channelName));

            if (temp == null) return;

            SelectedChannel = temp;
            NotifyPropertyChanged("SelectedChannel");
        }

        private void NewServerButtonClick() {
            int temp;
            if (string.IsNullOrEmpty(ServerAddress) ||
                string.IsNullOrEmpty(ServerPort) ||
                !int.TryParse(ServerPort, out temp)) return;

            NewServer(ServerAddress, temp);
            ServerAddress = ServerPort = string.Empty;
        }

        public void NewServer(string address, int port) {
            Server newServer = new Server(address, port, UpdateChannelNamesCollection);
            Servers.Add(newServer);
        }

        private void NotifyPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        // if this is called when server is being constructed then Servers list has no entries
        private void UpdateChannelNamesCollection(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (Channel channel in
                        e.NewItems.Cast<Channel>().Where(viewModel => !ChannelNames.Contains(viewModel.Name))) {
                        ChannelNames.Add(channel.Name);
                        SelectedChannelName = channel.Name;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Channel viewModel in
                        e.NewItems.Cast<Channel>().Where(viewModel => ChannelNames.Contains(viewModel.Name))) {
                        ChannelNames.Remove(viewModel.Name);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public MainViewModel() {
            NewServer("irc.foonetic.net", 6667);
        }
    }
}