using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avocado.Model;

namespace Avocado {
    public class MainViewModel : INotifyPropertyChanged {
        public const int BUFFER_SIZE = 20;
        private ChannelViewModel selectedChannel;
        private string selectedChannelName;

        private string serverAddress;
        private string serverPort;

        public MainViewModel() {
            NewServer("irc.foonetic.net", 6667);
        }

        public static string Nickname { get; private set; } = "TestIRCNickname";
        public static string Realname { get; private set; } = "TestIRCRealname";

        public string ServerAddress {
            get { return serverAddress; }
            set {
                if (value.Equals(serverAddress)) return;

                serverAddress = value;
                NotifyPropertyChanged("ServerAddress");
            }
        }

        public string ServerPort {
            get { return serverPort; }
            set {
                int temp;

                if (!int.TryParse(value, out temp) &&
                    !string.IsNullOrEmpty(value)) return;

                serverPort = value;
                NotifyPropertyChanged("ServerPort");
            }
        }

        public List<Server> Servers { get; } = new List<Server>();

        public ObservableCollection<string> ChannelNames { get; } = new ObservableCollection<string>();

        public string SelectedChannelName {
            get { return selectedChannelName; }
            set {
                if (string.IsNullOrEmpty(value) ||
                    value.Equals(selectedChannelName)) return;

                selectedChannelName = value;
                SetSelectedChannel(value);
                NotifyPropertyChanged("SelectedChannelName");
            }
        }

        public ICommand NewServerButtonCommand => new ActionCommand(NewServerButtonClick);

        public ChannelViewModel SelectedChannel {
            get { return selectedChannel; }
            set {
                if (value == selectedChannel) return;

                selectedChannel = value;
                NotifyPropertyChanged("SelectedChannel");
                Debug.WriteLine($"Selected channel changed to {value.Channel.Name}", "Information");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetSelectedChannel(string channelName) {
            Channel temp =
                Servers.SelectMany(server => server.Channels).FirstOrDefault(channel => channel.Name.Equals(channelName));

            if (temp == null) return;

            SelectedChannel = new ChannelViewModel(temp);
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

        // if this is called when server is being constructed then Channels list has no entries
        private void UpdateChannelNamesCollection(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (Channel channel in
                        e.NewItems.Cast<Channel>().Where(channel => !ChannelNames.Contains(channel.Name))) {
                        ChannelNames.Add(channel.Name);
                        SelectedChannelName = channel.Name;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Channel channel in
                        e.OldItems.Cast<Channel>().Where(channel => ChannelNames.Contains(channel.Name))) {
                        ChannelNames.Remove(channel.Name);
                        SelectedChannelName = ChannelNames.LastOrDefault();
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
    }
}