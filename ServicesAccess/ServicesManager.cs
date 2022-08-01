using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.ServiceProcess;
using WpfAppServices.Model;
using System.Diagnostics;
using WpfAppServices.Properties;
using WpfAppServices.ResourceHelpers;

namespace WpfAppServices.ServicesAccess {
    class ServicesManager {

        #region Thread Synchronization members
        /// <summary>
        /// Thread-safe singleton
        /// </summary>
        private static readonly Lazy<ServicesManager> lazy = new Lazy<ServicesManager>(() => new ServicesManager());
        public static ServicesManager Instance => lazy.Value;

        /// <summary>
        /// lock for m_services, access from Update Services worker and from UI
        /// </summary>
        private readonly object m_servicesLock = new object();

        /// <summary>
        /// Dispatcher to switch in UI thread from Update Service worker
        /// </summary>
        private readonly Dispatcher m_uiDispatcher = Application.Current.Dispatcher; // getting ui thread

        /// <summary>
        /// Timer raises Update Services task: handles changes in services (status, new services),
        /// updates m_services dictionary.
        /// </summary>
        private System.Timers.Timer m_updateServicesTimer = new System.Timers.Timer(3000);

        /// <summary>
        /// Timer raises Changed Services task: switch to UI thread and notify UI about changes
        /// </summary>
        private System.Timers.Timer m_changedServicesTimer = new System.Timers.Timer(50);

        #endregion Thread Syncronization members

        /// <summary>
        /// Dictionary for system services: key - ServiceName, value - ServiceDescModel
        /// Dictionary allows to manage changes in services (status or new services were added)
        /// </summary>
        private Dictionary<string, ServiceDescModel> m_services = new Dictionary<string, ServiceDescModel>();

        /// <summary>
        /// Event to notify subscribers (UI) about changes in Services (status, new services etc.)
        /// </summary>
        public event EventHandler<ChangedServicesEventArgs> ServicesChanged;

        /// <summary>
        /// Event to notify show/hide progress bar (passes task description)
        /// </summary>
        public event EventHandler<ServiceHandleTaskInProgressArgs> ServiceHandleTaskInProgressChanged;

        /// <summary>
        /// Event to notify about occured errors
        /// </summary>
        public event EventHandler<ServiceManagerErrorArgs> ServiceManagerErrorOccured;

        /// <summary>
        /// Constructor
        /// </summary>
        private ServicesManager() {

            // Disposing timers: it's supposed that ServicesManager should exist during all life cycle of application
            // Resoureces of timers will be disposed on application exit

            // set timers
            m_changedServicesTimer.Elapsed += OnChangeServicesTimedEvent;
            m_changedServicesTimer.AutoReset = true;
            m_changedServicesTimer.Enabled = false;

            // no need to create separate task for update services work
            // it will be handle in separate task due to timer is a separate worker
            m_updateServicesTimer.Elapsed += OnUpdateServicesTimedEvent;
            m_updateServicesTimer.AutoReset = false;
            m_updateServicesTimer.Enabled = true;

            //Task.Run(() => UpdateServices()); // first time request updating services
        }

        /// <summary>
        /// Update services timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateServicesTimedEvent(object sender, System.Timers.ElapsedEventArgs e) {
            UpdateServices();
            m_updateServicesTimer.Start();
        }

        /// <summary>
        /// Update Services: method does updating smart.
        /// It raises changed event in case of actual changes (status, description, name changes or adding/remove services)
        /// </summary>
        private void UpdateServices() {

            bool isServicesChanged = false;

            lock (m_servicesLock) {

                // TODO: do we need to dispose servicecontroller???
                try {
                    ServiceController[] srvControllerArr = ServiceController.GetServices();
                    if (srvControllerArr.Length < m_services.Count){
                        // recreate dictionary to handle of removed service case
                        m_services = new Dictionary<string, ServiceDescModel>();
                        isServicesChanged = true;
                    }

                    foreach (var srv in srvControllerArr) {
                        var srvDesc = string.Empty;
                        var srvStartName = string.Empty;
                        using (var serviceObject = new System.Management.ManagementObject(new System.Management.ManagementPath(string.Format("Win32_Service.Name='{0}'", srv.ServiceName)))) {
                            // obtain description and account
                            srvDesc = serviceObject["Description"]?.ToString();
                            srvStartName = serviceObject["StartName"]?.ToString();
                        }

                        var srvModel = new ServiceDescModel() { Name = srv.ServiceName, DisplayName = srv.DisplayName, Description = srvDesc, State = srv.Status.ToString(), Account = srvStartName };
                        if (!m_services.ContainsKey(srv.ServiceName) || m_services[srv.ServiceName] != srvModel) {
                            // new service - add to our dictionary
                            // or service data was changed
                            m_services[srv.ServiceName] = srvModel;
                            isServicesChanged = true;
                        }
                    }
                }
                catch (Exception e) {
                    Trace.TraceError($"[{e.GetType()}]: {e.Message}\r\n{e.StackTrace}");

                    m_uiDispatcher.BeginInvoke(new Action(() =>
                        throw e // something goes really bad
                    ));
                    throw e;
                }
            }

            // initiate notification
            if (isServicesChanged) {
                OnServicesChanged();
            }
        }

        /// <summary>
        /// Run service
        /// </summary>
        /// <param name="srvName">Service name to run</param>
        public void RunService(string srvName) {
            ManageServiceStatus(srvName, ServiceControllerStatus.Running);
        }

        /// <summary>
        /// Stop service
        /// </summary>
        /// <param name="srvName">Service name to stop</param>
        public void StopService(string srvName) {
            ManageServiceStatus(srvName, ServiceControllerStatus.Stopped);
        }

        /// <summary>
        /// Pause service
        /// </summary>
        /// <param name="srvName">Service name to pause</param>
        public void PauseService(string srvName) {
            ManageServiceStatus(srvName, ServiceControllerStatus.Paused);
        }

        /// <summary>
        /// Manage service status: run, stop or pause
        /// </summary>
        /// <param name="srvName">Service to manage</param>
        /// <param name="newStatus">Desired new status of service (running, paused or stropped)</param>
        private void ManageServiceStatus(string srvName, ServiceControllerStatus newStatus) {
            ServiceHandleTaskInProgressChanged(this,
                new ServiceHandleTaskInProgressArgs(ResourceHelper.GetTaskDescription(srvName, newStatus), true));

            Task.Run(() => {
                try {
                    // getting service controller by service name
                    var service = ServiceController.GetServices().FirstOrDefault((s) => string.Compare(s.ServiceName, srvName, true, CultureInfo.InvariantCulture) == 0);
                    if (service == null) throw new ArgumentException(ResourceHelper.GetManageServiceStatus_ArgumentException(srvName, newStatus));

                    switch (newStatus) {
                        case ServiceControllerStatus.Running:
                            // run command
                            if (service.Status == ServiceControllerStatus.Paused) service.Continue();
                            else service.Start();
                            break;

                        case ServiceControllerStatus.Paused:
                            // pause command
                            service.Pause();
                            break;

                        case ServiceControllerStatus.Stopped:
                            // stop command
                            service.Stop();
                            break;

                        default:
                            // unsupported command
                            throw new ArgumentException(string.Format(Resources.ManageServiceStatus_ArgumentException2, newStatus));
                    }

                    service.WaitForStatus(newStatus, new TimeSpan(0, 0, 30));
                    UpdateServices();
                }
                catch (ArgumentException e) {
                    // wrong srvName or newStatus arguments: null, service doesn't exist or unsupported command
                    Trace.TraceError($"[{e.GetType()}]: {e.Message}");
                    m_uiDispatcher.Invoke(() =>
                        ServiceManagerErrorOccured(this, new ServiceManagerErrorArgs(e.Message))
                    );
                }
                catch (System.ServiceProcess.TimeoutException e) {
                    // timeout of WaitForStatus
                    Trace.TraceWarning($"[{e.GetType()}]: {e.Message}");
                    m_uiDispatcher.Invoke(() =>
                        ServiceManagerErrorOccured(this, new ServiceManagerErrorArgs(Resources.ManageServiceStatus_TimeoutException))
                    );
                }
                catch (InvalidOperationException e) {
                    // command can't be executed due to problems with rights or with service options:
                    // need admin rights, serivice is disabled etc.
                    string additionalInformation = e.InnerException != null ? e.InnerException.Message : e.Message;
                    Trace.TraceError($"[{e.GetType()}]: {e.Message}. Inner Exception info: {additionalInformation}\r\n{e.StackTrace}");
                    m_uiDispatcher.Invoke(() =>
                        ServiceManagerErrorOccured(this, new ServiceManagerErrorArgs(
                            string.Format(Resources.ManageServiceStatus_InvalidOperationException, additionalInformation)))
                    );
                }
                catch (Exception e) {
                    // other problems
                    Trace.TraceError($"[{e.GetType()}]: {e.Message}\r\n{e.StackTrace}");
                    m_uiDispatcher.BeginInvoke(new Action(() =>
                        throw e // something gone unexpecdetelly bad
                    ));
                    throw e;
                }
                finally {
                    // notify ui to stop progress bar
                    m_uiDispatcher.Invoke(() =>
                        ServiceHandleTaskInProgressChanged(this, new ServiceHandleTaskInProgressArgs(string.Empty, false))
                    );
                }
            });
        }

        /// <summary>
        /// Timer event: new attempt getting acces to m_servicesLock resource
        /// UI thread can't wait while resource free, it will make attempts by timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChangeServicesTimedEvent(object sender, System.Timers.ElapsedEventArgs e) {
            OnServicesChanged();
        }

        /// <summary>
        /// UI-thread method. Method notifies UI about occured changes in Services List (new service or changed statuses)
        /// </summary>
        private void OnServicesChanged() {

            // switch to ui thread
            m_uiDispatcher.Invoke(() => {

                m_changedServicesTimer.Enabled = false;

                // don't wait in UI thread
                if (Monitor.TryEnter(m_servicesLock, 100)) {
                    try {
                        if (ServicesChanged != null) {
                            // notify UI about changes
                            ServicesChanged(this, new ChangedServicesEventArgs(
                                new ObservableCollection<ServiceDescModel>(m_services.Select(s => s.Value)))
                                );
                        }
                    }
                    finally {
                        Monitor.Exit(m_servicesLock);
                        //m_updateServicesTimer.Enabled = true; // reenable updating services
                    }
                }
                else {
                    // try again getting access to resource (m_services)
                    m_changedServicesTimer.Enabled = true;
                }

            });
        }
    }
}
