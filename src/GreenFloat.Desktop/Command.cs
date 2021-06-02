using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GreenFloat.Desktop
{
    class Command : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Func<object, bool> canExecuteFunc;
        private Action<object> executeAction;

        public Command(Func<object,bool> canExecute, Action<object> execute)
        {
            canExecuteFunc = canExecute;
            executeAction = execute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecuteFunc(parameter);
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
    }
}
