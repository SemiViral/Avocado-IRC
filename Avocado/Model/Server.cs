using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Avocado.Model.Messages;

namespace Avocado.Model {
    public class Server : INotifyPropertyChanged {
        private static readonly string[] IrrelevantNicknames = {"NickServ", "ChanServ", MainViewModel.Nickname};

        private string _hostname;

        public ObservableCollection<Channel> Channels { get; } = new ObservableCollection<Channel>();

        public Server(string address, int port, Action<object, NotifyCollectionChangedEventArgs> channelAddedAction) {
            Channels.CollectionChanged += (sender, args) => channelAddedAction(sender, args);
            Hostname = address;
            Port = port;

            Connection = new Connection(address, port);
            Connection.MessageRecieved += (sender, message) => DispatchAction(() => OnMessageRecieved(message));

            Thread connectionThread = new Thread(Connection.Listen) {
                IsBackground = true
            };
            connectionThread.Start();

            PreConnect();

            ShouldRun = true;
        }

        public bool ShouldRun { get; private set; }

        private bool IsVerified { get; set; }

        public string Hostname {
            get { return _hostname; }
            set {
                if (value == _hostname) return;

                _hostname = value;
                NotifyPropertyChanged("Hostname");
            }
        }

        public string BaseHost => GetBaseHost(Hostname, true);

        public int Port { get; private set; }

        public Connection Connection { get; }

        //public ObservableCollection<ChannelViewModel> Channels { get; } = new ObservableCollection<ChannelViewModel>();

        //public ObservableCollection<ChannelViewModel> ShortChannels {
        //    get { return new ObservableCollection<ChannelViewModel>(Channels.Where(chan => chan.Name != Hostname)); }
        //}

        public event PropertyChangedEventHandler PropertyChanged;

        public void PreConnect() {
            Connection.Write(new OutputMessage("USER", $"{MainViewModel.Nickname} 0 * {MainViewModel.Realname}"));
            Connection.Write(new OutputMessage("NICK", MainViewModel.Nickname));
        }

        public void CreateChannel(string name) {
            Channel temp = new Channel(name);

            if (Channels.Contains(temp)) return;

            temp.SendMessageEvent += SendMessageOnEvent;
            Channels.Add(temp);
        }

        public static string GetBaseHost(string hostname, bool stripDomainExtension = false) {
            if (string.IsNullOrEmpty(hostname)) return null;
            string baseHost = hostname.Substring(hostname.IndexOf(".", StringComparison.Ordinal) + 1);

            return stripDomainExtension && baseHost.Contains(".")
                ? baseHost.Substring(0, baseHost.IndexOf(".", StringComparison.Ordinal)) : baseHost;
        }

        public Channel GetChannel(string name) {
            return Channels.FirstOrDefault(channel => channel.Name.Equals(name));
        }

        public static void DispatchAction(Action action) {
            if (Application.Current == null) return;

            if (Application.Current.Dispatcher.CheckAccess()) action?.Invoke();
            else Application.Current.Dispatcher?.Invoke(DispatcherPriority.Normal, action);
        }

        public void DisplayMessage(Message message) {
            // if this code is every called the channel should exist,
            // as CheckCreateChannel should be called first
            if (string.IsNullOrEmpty(message?.Args)) return;
            GetChannel(message.Target).AppendMessage(message);
        }

        private void OnMessageRecieved(ChannelMessage message) {
            if (message.IsPing) {
                Connection.Write(message.Args);
                return;
            }

            PerformPreprocessing(message);

            Debug.WriteLine(message.RawMessage, "Output");
            DisplayMessage(ProcessMessage(message));
        }

        private void PerformPreprocessing(ChannelMessage message) {
            // check if message target is updated hostname
            if (GetBaseHost(message.Target).Equals(GetBaseHost(Hostname))) {
                Connection.Address = message.Target;
                Hostname = message.Target;
                message.Target = BaseHost;
            }

            // check if channel needs to be created from target
            if ((message.Target.Equals(BaseHost) ||
                 message.Target.StartsWith("#")) &&
                GetChannel(message.Target) == null) CreateChannel(message.Target);

            // Check if target needs to be BaseHost
            if (!message.IsRealUser ||
                IrrelevantNicknames.Contains(message.Target)) { // if target is server
                message.Target = BaseHost;
            }
        }

        private void SendMessageOnEvent(object sender, Message message) {
            Connection.Write(message);
        }

        private void NotifyPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private Message ProcessMessage(ChannelMessage message) {
            switch (message.Type) {
                case "NOTICE":
                    return new Message(message.Nickname, message.Target, message.Args);
                case "MODE":
                    if (!IsVerified) IsVerified = !IsVerified;
                    return new Message(message.Nickname, message.Target, message.Args);
                case "PART":
                    if (message.Target.StartsWith("#") &&
                        GetChannel(message.Target) != null) Channels.Remove(GetChannel(message.Target));
                    break;
                case "353": // names reply
                    message.Target = message.SplitArgs[1]; // SplitArgs[1] is the targeted channel in this instance
                    Channel channel = GetChannel(message.Target);

                    foreach (string s in message.Args.Trim().Split(':')[1].Split(' ')) {
                        channel.AddUser(s);
                    }

                    return new Message(message.Nickname, message.Target,
                        string.Concat("Users in this channel: ",
                            string.Join(", ", channel.Users.Select(user => user.Name).ToList())));
                case "366": // end of names reply
                    // in this instance, SplitArgs[0] is the target channel
                    //SelectedChannel = GetChannel(message.SplitArgs[0]);
                    break;
                default:
                    return new Message(message.Nickname, message.Target, message.Args);
            }

            return null;
        }
    }
}