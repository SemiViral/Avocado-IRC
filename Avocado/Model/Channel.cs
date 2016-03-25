using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avocado.Model.Messages;

namespace Avocado.Model {
    public class Channel {
        public Channel(string name) {
            Name = name;

            if (!Name.StartsWith("#")) IsPrivate = true;

            MessagesArchive.CollectionChanged += SizeMessageBuffer;

            Debug.WriteLine($"Channel {Name} created", "Information");
        }

        public bool IsPrivate { get; }

        public string Name { get; set; }

        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>();
        private ObservableCollection<Message> MessagesArchive { get; } = new ObservableCollection<Message>();

        private void SizeMessageBuffer(object sender, NotifyCollectionChangedEventArgs e) {
            if (Messages.Count + e.NewItems.Count > MainViewModel.BUFFER_SIZE) {
                foreach (Message message in e.NewItems) {
                    Messages.RemoveAt(0);
                    Messages.Add(message);
                }
            } else {
                foreach (Message message in e.NewItems) {
                    Messages.Add(message);
                }
            }
        }

        public void AppendMessage(Message message) {
            MessagesArchive.Add(message);
        }

        public void AddUser(string name) {
            Users.Add(new User(name));
        }

        public event MessageEventHandler SendMessageEvent;

        public void InvokeMessage(Message message) {
            SendMessageEvent?.Invoke(this, message);
        }
    }
}