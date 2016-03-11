using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avocado.Model;

namespace Avocado.ViewModel {
	public class ChannelViewModel {
		public string SendText { get; set; }
		public string Name { get; set; }

		public ObservableCollection<ChannelUser> Users { get; } = new ObservableCollection<ChannelUser>(); 
		public ObservableCollection<Message> Messages { get; } = new ObservableCollection<Message>(); 
		public List<Message> TempraryArchive = new List<Message>();

		public event EventHandler<Message> SendMessageEvent;

		public ChannelViewModel(string name) {
			Name = name;
		}

		public void AppendMessage(Message message) {
			Messages.Add(message);
		}

		public void TextBoxSend_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key != Key.Enter) return;
			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) return;

			if (SendText.StartsWith("/")) {
				string[] splitText = SendText.Split(new[] {' '}, 2);
				SendMessageEvent?.Invoke(this, new Message(splitText[0].Substring(1).ToUpper(), splitText[1]));
			} else {
				SendMessageEvent?.Invoke(this, new Message("PRIVMSG", Name, SendText.Trim()));
			}

			e.Handled = true;
		}
	}
}
