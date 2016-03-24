namespace Avocado.Model.Messages {
    public class OutputMessage : Message {
        public OutputMessage(string type, string target, string args) {
            Type = type;
            Target = target;
            Args = args;

            RawMessage = $"{Type} {Target} {Args}";
        }

        public OutputMessage(string type, string args) {
            Type = type;
            Args = args;

            RawMessage = $"{Type} {Args}";
        }
    }
}