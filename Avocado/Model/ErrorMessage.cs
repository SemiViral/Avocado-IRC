namespace Avocado.Model {
    public class ErrorMessage : Message {
        /// <summary>
        ///     Use this constructor for displaying error messages
        /// </summary>
        /// <param name="targetTab">TargetTab tab</param>
        /// <param name="message"></param>
        public ErrorMessage(string targetTab, string message) {
            Nickname = "Error";
            Target = targetTab;
            Args = message;
        }
    }
}