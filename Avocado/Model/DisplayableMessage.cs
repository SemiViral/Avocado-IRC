using System;

namespace Avocado.Model {
	public class DisplayableMessage {
		public DisplayableMessage(string nickname, string targetTab, string message) {
			Nickname = nickname;
			TargetTab = targetTab;
			Message = message;
		}

		/// <summary>
		///     Use this constructor for displaying error messages
		/// </summary>
		/// <param name="targetTab">TargetTab tab</param>
		/// <param name="message"></param>
		public DisplayableMessage(string targetTab, string message) {
			IsErrorMessage = true;
			Nickname = "Error";
			TargetTab = targetTab;
			Message = message;
		}

		public DisplayableMessage(Message message) {
			Nickname = message.Nickname;
			TargetTab = message.Target;
			Message = message.Args;
		}

		public DisplayableMessage() {
			DoDisplay = false;
		}

		public string Nickname { get; }
		public string TargetTab { get; }
		public string Message { get; }
		public bool IsErrorMessage { get; }
		public bool DoDisplay { get; } = true;

		public string Timestamp { get; } = DateTime.Now.ToShortTimeString();
	}
}