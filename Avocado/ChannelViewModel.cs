using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Avocado.Model;
using Avocado.Model.Messages;

namespace Avocado {
    public class ChannelViewModel : INotifyPropertyChanged {
        private static readonly SortDescription AccessLevelSort = new SortDescription("AccessLevel",
            ListSortDirection.Ascending);

        private readonly List<string> _pastInputs = new List<string>();
        private Channel _channel;

        private string _sendText;

        public ChannelViewModel(Channel baseChannel) {
            Channel = baseChannel;

            UsersView.Source = Channel.Users;
            UsersView.SortDescriptions.Add(AccessLevelSort);
        }

        public CollectionViewSource UsersView { get; } = new CollectionViewSource();

        public Channel Channel {
            get { return _channel; }
            set {
                if (_channel == value) return;

                _channel = value;
                NotifyPropertyChanged("Channel");
            }
        }

        public string SendText {
            get { return _sendText; }
            set {
                if (value == _sendText) return;

                _sendText = value;
                NotifyPropertyChanged("SendText");
            }
        }

        public ICommand KeyUp_Enter_Command => new ActionCommand(KeyUp_Enter);

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
                _pastInputs.Add(SendText);
            }

            SendText = string.Empty;
        }
    }
}