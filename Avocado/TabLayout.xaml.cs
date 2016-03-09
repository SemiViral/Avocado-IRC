using System;
using System.Windows.Input;
using Avocado.IRC;
using Avocado.IRC.Types;

namespace Avocado {
	/// <summary>
	/// Interaction logic for TabLayout.xaml
	/// </summary>
	public partial class TabLayout {
		public string TabName { get; }
		public int ServerIndex { get; }
		public Guid UniqueId { get; }

		public event EventHandler<MessageEventArgs> SendMessageEvent;

		public TabLayout(int serverIndex, string tabName) {
			InitializeComponent();

			ServerIndex = serverIndex;
			TabName = tabName;

			UniqueId = Guid.NewGuid();
		}

		public void Output(string text) {
			MainWindow.DispatchAction(() => {
				RichTextBoxPrimary.AppendText(string.Concat(text.Trim(), "\n"));
			});
		}

		public void AddUserToList(ChannelUser user) {
			UsersListView.Items.Add(user);
		}

		private void TextBoxSend_KeyUp(object sender, KeyEventArgs e) {
			if (e.Key != Key.Enter) return;

			if (TextBoxSend.Text.StartsWith("/")) {
				string[] splitText = TextBoxSend.Text.Split(new[] {' '}, 2);
				SendMessageEvent?.Invoke(this, new MessageEventArgs(splitText[0].Substring(1).ToUpper(), string.Empty, splitText[1]));
			} else {
				SendMessageEvent?.Invoke(this, new MessageEventArgs("PRIVMSG", TabName, TextBoxSend.Text.Trim()));
			}

			e.Handled = true;
		}
	}
}
