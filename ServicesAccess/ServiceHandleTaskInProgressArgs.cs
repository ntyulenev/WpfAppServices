using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppServices.ServicesAccess {
    class ServiceHandleTaskInProgressArgs : EventArgs {

        public ServiceHandleTaskInProgressArgs(string taskDescription, bool isInProgress) {
            TaskDescription = taskDescription;
            IsInProgress = isInProgress;
        }

        /// <summary>
        /// Task description to show in UI
        /// </summary>
        public string TaskDescription { get; set; }

        /// <summary>
        /// Task status: true - in progress, false - finished
        /// </summary>
        public bool IsInProgress { get; set; }
    }
}
