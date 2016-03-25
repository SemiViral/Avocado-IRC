using System;
using System.Windows.Input;

namespace Avocado.Model {
    internal class ActionCommand : ICommand {
        private readonly Action action;

        public ActionCommand(Action action) {
            this.action = action;
        }

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            action();
        }

        public event EventHandler CanExecuteChanged;
    }
}