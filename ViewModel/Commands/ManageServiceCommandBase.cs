using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfAppServices.ViewModel.Commands {
    abstract class ManageServiceCommandBase : ICommand {

        public event EventHandler CanExecuteChanged;

        public void OnChangedSelectedService() {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, new EventArgs());
        }

        // block command until other command executing
        // it's not neccessary, commands can be executed in paralel
        // but it's required additional complex code to make UI consistent
        private bool m_blockCanExecute = false;
        public bool BlockCanExecute {
            get => m_blockCanExecute;
            set {
                m_blockCanExecute = value;
                if (CanExecuteChanged != null)
                    CanExecuteChanged(this, new EventArgs());
            }
        }

        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);
    }
}
