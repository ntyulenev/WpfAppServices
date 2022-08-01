using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppServices.ServicesAccess {
    class ServiceManagerErrorArgs {

        public ServiceManagerErrorArgs(string errorMsg) {
            ErrorMessage = errorMsg;
        }

        public string ErrorMessage { get; set; }
    }
}
