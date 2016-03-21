using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Avocado.Model;

namespace Avocado.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        private Visibility _newServerDialogueVisible = Visibility.Visible;
        private ChannelViewModel _selectedChannel;
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
                int test;
                bool isNull = string.IsNullOrEmpty(value);

                if (!int.TryParse(value, out test) &&
                    !isNull) return;

                _serverPort = isNull ? string.Empty : value;
                NotifyPropertyChanged("ServerPort");
            }
        }

        public ObservableCollection<ServerViewModel> Servers { get; } =
            new ObservableCollection<ServerViewModel>();

        public ObservableCollection<string> ChannelNames { get; } = new ObservableCollection<string>();

        public string SelectedChannelName {
            get { return _selectedChannelName; }
            set {
                if (value.Equals(_selectedChannelName) ||
                    string.IsNullOrEmpty(value)) return;

                _selectedChannelName = value;
                NotifyPropertyChanged("SelectedChannelName");

                SetSelectedChannel(value);
            }
        }

        public ICommand NewServerButtonClick => new ActionCommand(NewServerButton);

        public Visibility NewServerDialogueVisible {
            get { return _newServerDialogueVisible; }
            set {
                _newServerDialogueVisible = value;
                NotifyPropertyChanged("NewServerDialogueVisible");
            }
        }

        public ChannelViewModel SelectedChannel {
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
            ChannelViewModel temp =
                Servers.SelectMany(server => server.Channels).FirstOrDefault(channel => channel.Name.Equals(channelName));

            if (temp == null) {
                if (ChannelNames.Contains(channelName)) ChannelNames.Remove(channelName);

                return;
            }

            SelectedChannel = temp;
            NotifyPropertyChanged("SelectedChannel");
        }

        private void NewServerButton() {
            int temp;
            if (string.IsNullOrEmpty(ServerAddress) ||
                string.IsNullOrEmpty(ServerPort) ||
                !int.TryParse(ServerPort, out temp)) return;

            NewServer(ServerAddress, temp);
            ServerAddress = ServerPort = string.Empty;
        }

        public void NewServer(string address, int port) {
            ServerViewModel temp = new ServerViewModel(address, port);
            temp.Channels.CollectionChanged += UpdateChannelNamesCollection;
            Servers.Add(temp);
        }

        private void NotifyPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void UpdateChannelNamesCollection(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach ( ChannelViewModel viewModel in
                            e.NewItems.Cast<ChannelViewModel>().Where(viewModel => ChannelNames.Contains(viewModel.Name))) {
                        ChannelNames.Add(viewModel.Name);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ChannelViewModel viewModel in
                            e.NewItems.Cast<ChannelViewModel>().Where(viewModel => ChannelNames.Contains(viewModel.Name))) {
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
    }
}