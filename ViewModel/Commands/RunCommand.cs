using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfAppServices.Model;
using WpfAppServices.ServicesAccess;

namespace WpfAppServices.ViewModel.Commands {
    class RunCommand : ManageServiceCommandBase {

        public override bool CanExecute(object parameter) {
            if (BlockCanExecute) return false;

            var srv = parameter as ServiceDescModel;
            if (srv == null) return false;
            return srv.State == "Stopped" || srv.State == "Paused";
        }

        public override void Execute(object parameter) {
            var srv = parameter as ServiceDescModel;
            ServicesManager.Instance.RunService(srv.Name);
        }
    }
}
