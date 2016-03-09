﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Avocado.IRC.Types {
	public class ChannelMessage {
		private static readonly Regex MessageRegex =
			new Regex(@"^:(?<Sender>[^\s]+)\s(?<Type>[^\s]+)\s(?<Target>[^\s]+)\s?:?(?<Args>.*)", RegexOptions.Compiled);

		private static readonly Regex SenderRegex = new Regex(@"^(?<Nickname>[^\s]+)!(?<Realname>[^\s]+)@(?<Hostname>[^\s]+)",
			RegexOptions.Compiled);

		public ChannelMessage(string rawData) {
			RawMessage = rawData;
			Parse(RawMessage);

			if (IsRealUser) return;

			Target = Hostname;
		}

		public string RawMessage { get; }
		public bool IsRealUser { get; private set; }

		public string Nickname { get; private set; }
		public string Realname { get; private set; }
		public string Hostname { get; private set; }
		public string Target { get; private set; }
		public string Type { get; set; }
		public string Args { get; private set; }
		public List<string> SplitArgs { get; private set; } = new List<string>();

		public void Parse(string rawData) {
			if (!MessageRegex.IsMatch(rawData)) return;

			// begin parsing message into sections
			Match messageMatch = MessageRegex.Match(rawData);
			Match senderMatch = SenderRegex.Match(messageMatch.Groups["Sender"].Value);

			// class property setting
			Nickname = messageMatch.Groups["Sender"].Value;
			Realname = messageMatch.Groups["Sender"].Value.ToLower();
			Hostname = messageMatch.Groups["Sender"].Value;
			Type = messageMatch.Groups["Type"].Value;
			Target = messageMatch.Groups["Target"].Value.StartsWith(":")
				? messageMatch.Groups["Target"].Value.Substring(1)
				: messageMatch.Groups["Target"].Value;
			Args = messageMatch.Groups["Args"].Value;
			SplitArgs = Args?.Trim().Split(new[] {' '}, 4).ToList();

			if (!senderMatch.Success) return;

			string realname = senderMatch.Groups["Realname"].Value;
			Nickname = senderMatch.Groups["Nickname"].Value;
			Realname = realname.StartsWith("~") ? realname.Substring(1) : realname;
			Hostname = senderMatch.Groups["Hostname"].Value;
			IsRealUser = true;
		}
	}
}
