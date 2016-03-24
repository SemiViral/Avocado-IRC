using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Avocado.Model.Messages;

namespace Avocado.Model {
    public class Channel {
        public Channel(string name) {
            Name = name;

            if (Name.StartsWith("#"))
                _users.CollectionChanged += OnUsersAddedSort;
            else IsPrivate = true;

            Debug.WriteLine($"Channel {Name} created", "Information");
        }

        public bool IsPrivate { get; }

        public string Name { get; set; }

        private ObservableCollection<User> _users { get; } = new ObservableCollection<User>();
        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();

        private void OnUsersAddedSort(object sender, NotifyCollectionChangedEventArgs e) {
            Users = new ObservableCollection<User>(_users.OrderBy(user => user.AccessLevel));
        }

        public void AppendMessage(Message message) {
            Messages.Add(message);
        }

        public void AddUser(string name) {
            _users.Add(new User(name));
        }

        public event MessageEventHandler SendMessageEvent;

        public void InvokeMessage(Message message) {
            SendMessageEvent?.Invoke(this, message);
        }
    }
}