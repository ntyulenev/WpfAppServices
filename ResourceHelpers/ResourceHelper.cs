using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WpfAppServices.Properties;

namespace WpfAppServices.ResourceHelpers {
    class ResourceHelper {

        /// <summary>
        /// Generates string for task description
        /// </summary>
        /// <param name="srvName"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public static string GetTaskDescription(string srvName, ServiceControllerStatus newStatus) {
            string res = string.Empty;

            switch (newStatus) {
                case ServiceControllerStatus.Running:
                    res = string.Format(Resources.ServiceTaskDesc_Running, srvName);
                    break;

                case ServiceControllerStatus.Stopped:
                    res = string.Format(Resources.ServiceTaskDesc_Stopped, srvName);
                    break;

                case ServiceControllerStatus.Paused:
                    res = string.Format(Resources.ServiceTaskDesc_Paused, srvName);
                    break;
            }

            return res;
        }

        /// <summary>
        /// Generates string for argument exception in ServiceManager.ManageServiceStatus
        /// </summary>
        /// <param name="srvName"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public static string GetManageServiceStatus_ArgumentException(string srvName, ServiceControllerStatus newStatus) {
            string res = string.Format(Resources.ManageServiceStatus_ArgumentException2, newStatus); // unsupported command

            switch (newStatus) {
                case ServiceControllerStatus.Running:
                    res = string.Format(Resources.ManageServiceStatus_ArgumentException_Running, srvName);
                    break;

                case ServiceControllerStatus.Stopped:
                    res = string.Format(Resources.ManageServiceStatus_ArgumentException_Stopping, srvName);
                    break;

                case ServiceControllerStatus.Paused:
                    res = string.Format(Resources.ManageServiceStatus_ArgumentException_Pausing, srvName);
                    break;
            }

            return res;
        }

        /// <summary>
        /// Generates string for exception in ServiceManager.ManageServiceStatus
        /// </summary>
        /// <param name="srvName"></param>
        /// <param name="newStatus"></param>
        /// <returns></returns>
        public static string GetManageServiceStatus_Exception(string srvName, ServiceControllerStatus newStatus) {
            string res = string.Format(Resources.ManageServiceStatus_ArgumentException2, newStatus); // unsupported command

            switch (newStatus) {
                case ServiceControllerStatus.Running:
                    res = string.Format(Resources.ManageServiceStatus_Exception_Running, srvName);
                    break;

                case ServiceControllerStatus.Stopped:
                    res = string.Format(Resources.ManageServiceStatus_Exception_Stopping, srvName);
                    break;

                case ServiceControllerStatus.Paused:
                    res = string.Format(Resources.ManageServiceStatus_Exception_Pausing, srvName);
                    break;
            }

            return res;
        }
    }
}
