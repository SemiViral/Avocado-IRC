using System;
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
		private ICommand _keyUpCommand;

		private string _sendText;
		public List<Message> TempraryArchive = new List<Message>();

		public ChannelViewModel(string name) {
			Name = name;
			_users.CollectionChanged += OnUsersAddedSort;

			Debug.WriteLine($"Channel {Name} created", "Information");
		}

		private void OnUsersAddedSort(object sender, NotifyCollectionChangedEventArgs e) {
			Users = new ObservableCollection<ChannelUser>(_users.OrderBy(user => user.AccessLevel));

		}

		public string SendText {
			get { return _sendText; }
			set {
				if (value == _sendText) return;

				_sendText = value;
				NotifyPropertyChanged("SendText");
			}
		}

		public string Name { get; set; }

		private List<string> PastInputs = new List<string>(); 

		public ObservableCollection<ChannelUser> _users { get; }= new ObservableCollection<ChannelUser>();  
		public ObservableCollection<ChannelUser> Users { get; private set; } = new ObservableCollection<ChannelUser>();
		public ObservableCollection<DisplayableMessage> Messages { get; } = new ObservableCollection<DisplayableMessage>();

		public ICommand KeyUp_Enter_Command {
			get {
				return _keyUpCommand ?? (_keyUpCommand = new ActionCommand(() => {
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) return;

					string sendString;

					if (SendText.StartsWith("/")) {
						string[] splitText = SendText.Split(new[] {' '}, 2);
						SendMessageEvent?.Invoke(this, new Message(splitText[0].Substring(1).ToUpper(), splitText[1]));
					} else {
						SendMessageEvent?.Invoke(this, new Message("PRIVMSG", Name, string.Concat(SendText, "\r\n")));
						Messages.Add(new DisplayableMessage(MainViewModel.Nickname, Name, SendText));
					}

					SendText = string.Empty;
				}));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public event EventHandler<Message> SendMessageEvent;

		public void AppendMessage(DisplayableMessage message) {
			Messages.Add(message);
		}

		public void AddUser(string name) {
			_users.Add(new ChannelUser(name));
		}

		private void NotifyPropertyChanged(string info) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
}