using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAppServices.Model;

namespace WpfAppServices.ServicesAccess {
    class ChangedServicesEventArgs : EventArgs {

        public ChangedServicesEventArgs(ObservableCollection<ServiceDescModel> services) {
            Services = services;
        }

        public ObservableCollection<ServiceDescModel> Services { get; set; }
    }
}
