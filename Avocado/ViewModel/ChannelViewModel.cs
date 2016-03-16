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
		private readonly List<string> _pastInputs = new List<string>();
		private ICommand _keyUpCommand;
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

		public ObservableCollection<ChannelUser> _users { get; } = new ObservableCollection<ChannelUser>();
		public ObservableCollection<ChannelUser> Users { get; private set; } = new ObservableCollection<ChannelUser>();
		public ObservableCollection<DisplayableMessage> Messages { get; } = new ObservableCollection<DisplayableMessage>();
		public ObservableCollection<string> SimpleMessages { get; } = new ObservableCollection<string>();

		public ICommand KeyUp_Enter_Command {
			get {
				return _keyUpCommand ?? (_keyUpCommand = new ActionCommand(() => {
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
						SendText += '\n';
						return;
					}

					if (string.IsNullOrEmpty(SendText)) return;

					if (SendText.StartsWith("/")) {
						List<string> splitText = SendText.Split(new[] {' '}, 2).ToList();

						SendMessageEvent?.Invoke(this, new Message(splitText[0].Substring(1).ToUpper(), splitText[1] ?? string.Empty));
					} else {
						SendMessageEvent?.Invoke(this, new Message("PRIVMSG", Name, string.Concat(SendText, "\r\n")));
						Messages.Add(new DisplayableMessage(MainViewModel.Nickname, Name, SendText));
						_pastInputs.Add(SendText);
					}

					SendText = string.Empty;
				}));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnUsersAddedSort(object sender, NotifyCollectionChangedEventArgs e) {
			Users = new ObservableCollection<ChannelUser>(_users.OrderBy(user => user.AccessLevel));
		}

		public event EventHandler<Message> SendMessageEvent;

		public void AppendMessage(DisplayableMessage message) {
			if (IsPrivate) {
				SimpleMessages.Add(message.Formatted);
			} else {
				Messages.Add(message);
			}
		}

		public void AddUser(string name) {
			_users.Add(new ChannelUser(name));
		}

		private void NotifyPropertyChanged(string info) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}
	}
}