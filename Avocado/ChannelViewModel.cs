using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Avocado.Model;
using Avocado.Model.Messages;

namespace Avocado {
    public class ChannelViewModel : INotifyPropertyChanged {
        private static readonly SortDescription _accessLevelSort = new SortDescription("AccessLevel",
            ListSortDirection.Ascending);

        private readonly List<string> pastInputs = new List<string>();
        private Channel channel;

        private string sendText;

        public ChannelViewModel(Channel baseChannel) {
            Channel = baseChannel;

            UsersView.Source = Channel.Users;
            UsersView.SortDescriptions.Add(_accessLevelSort);
        }

        public CollectionViewSource UsersView { get; } = new CollectionViewSource();

        public Channel Channel {
            get { return channel; }
            set {
                if (channel == value) return;

                channel = value;
                NotifyPropertyChanged("Channel");
            }
        }

        public string SendText {
            get { return sendText; }
            set {
                if (value == sendText) return;

                sendText = value;
                NotifyPropertyChanged("SendText");
            }
        }

        public ICommand KeyUpEnterCommand => new ActionCommand(KeyUp_Enter);

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private void KeyUp_Enter() {
            if (string.IsNullOrEmpty(SendText)) return;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                SendText += '\n';
                return;
            }

            if (SendText.StartsWith("/")) {
                List<string> splitText = SendText.Split(new[] {' '}, 2).ToList();

                Channel.InvokeMessage(new OutputMessage(splitText[0].Substring(1).ToUpper(), splitText[1] ?? string.Empty));
            } else {
                Channel.InvokeMessage(new OutputMessage("PRIVMSG", Channel.Name, string.Concat(SendText, "\r\n")));
                Channel.AppendMessage(new Message(MainViewModel.Nickname, Channel.Name, SendText));
                pastInputs.Add(SendText);
            }

            SendText = string.Empty;
        }
    }
}