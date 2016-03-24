using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Avocado.Model.Messages {
    internal class Regexes {
        public static readonly Regex Message =
            new Regex(@"^:(?<Sender>[^\s]+)\s(?<Type>[^\s]+)\s(?<Target>[^\s]+)\s?:?(?<Args>.*)", RegexOptions.Compiled);

        public static readonly Regex Sender = new Regex(@"^(?<Nickname>[^\s]+)!(?<Realname>[^\s]+)@(?<Hostname>[^\s]+)",
            RegexOptions.Compiled);
    }

    public class ChannelMessage : Message {
        public ChannelMessage(string rawData) {
            RawMessage = rawData;

            if (CheckPing() ||
                !Regexes.Message.IsMatch(rawData)) return;

            Parse(RawMessage);

            if (IsPing || IsRealUser) return;

            Target = Hostname;
        }

        public bool IsRealUser { get; private set; }
        public bool IsPing { get; private set; }

        public string Realname { get; private set; }
        public string Hostname { get; private set; }

        public List<string> SplitArgs { get; private set; } = new List<string>();

        public void Parse(string rawData) {
            // begin parsing message into sections
            Match messageMatch = Regexes.Message.Match(rawData);
            Match senderMatch = Regexes.Sender.Match(messageMatch.Groups["Sender"].Value);

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

            Nickname = senderMatch.Groups["Nickname"].Value;
            Realname = senderMatch.Groups["Realname"].Value;
            Hostname = senderMatch.Groups["Hostname"].Value;
            IsRealUser = true;
        }

        private bool CheckPing() {
            if (!RawMessage.StartsWith("PING")) return false;

            IsPing = true;
            Args = string.Concat("PONG ", RawMessage.Substring(5));
            return true;
        }
    }
}