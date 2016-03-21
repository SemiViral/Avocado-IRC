using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Avocado.Model;

namespace Avocado.ViewModel {
    public class ChannelViewModel : INotifyPropertyChanged {
        private readonly List<string> _pastInputs = new List<string>();
        private string _sendText;
        public List<Message> TempraryArchive = new List<Message>();

        public ChannelViewModel(string name) {
            Name = name;

            if (Name.StartsWith("#"))
                _users.CollectionChanged += OnUsersAddedSort;
            else IsPrivate = true;

            Debug.WriteLine($"Channel {Name} created", "Information");
        }

        public bool IsPrivate { get; }

        public string SendText {
            get { return _sendText; }
            set {
                if (value == _sendText) return;

                _sendText = value;
                NotifyPropertyChanged("SendText");
            }
        }

        public string Name { get; set; }

        public ObservableCollection<User> _users { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> Users { get; set; }
                public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        public ICommand KeyUp_Enter_Command => new ActionCommand(KeyUp_Enter);

        public event PropertyChangedEventHandler PropertyChanged;

        private void KeyUp_Enter() {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                SendText += '\n';
                return;
            }

            if (string.IsNullOrEmpty(SendText)) return;

            if (SendText.StartsWith("/")) {
                List<string> splitText = SendText.Split(new[] {' '}, 2).ToList();

                SendMessageEvent?.Invoke(this,
                    new OutputMessage(splitText[0].Substring(1).ToUpper(), splitText[1] ?? string.Empty));
            } else {
                SendMessageEvent?.Invoke(this, new OutputMessage("PRIVMSG", Name, string.Concat(SendText, "\r\n")));
                Messages.Add(new Message(MainViewModel.Nickname, Name, SendText));
                _pastInputs.Add(SendText);
            }

            SendText = string.Empty;
        }

        private void OnUsersAddedSort(object sender, NotifyCollectionChangedEventArgs e) {
            Users = new ObservableCollection<User>(_users.OrderBy(user => user.AccessLevel));
        }

        public event MessageEventHandler SendMessageEvent;

        public void AppendMessage(Message message) {
            Debug.WriteLine(message.Args);
            Messages.Add(message);
        }

        public void AddUser(string name) {
            _users.Add(new User(name));
        }

        private void NotifyPropertyChanged(string info) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}