using System;

namespace Avocado.Model {
    public class Message {
        protected Message() {}

        /// <summary>
        ///     For adding simple messages to list
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="target"></param>
        /// <param name="message"></param>
        public Message(string nickname, string target, string message) {
            Nickname = nickname;
            Target = target;
            Args = message;

            RawMessage = $"{Type} {Target} {Args}";
        }

        public string RawMessage { get; set; }

        public string Nickname { get; set; }
        public string Target { get; set; }
        public string Type { get; set; }
        public string Args { get; set; }

        public string Timestamp { get; } = DateTime.Now.ToShortTimeString();
    }
}