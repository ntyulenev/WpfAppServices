using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfAppServices.Model;
using WpfAppServices.ServicesAccess;
using WpfAppServices.ViewModel.Commands;

namespace WpfAppServices.ViewModel {
    class MainWindowViewModel : INotifyPropertyChanged {

        private ServicesManager m_servicesHandler = ServicesManager.Instance;

        public MainWindowViewModel() {
            m_servicesHandler.ServicesChanged += OnServicesChanged; // list of services was changed (add/remove service, status etc.)
            m_servicesHandler.ServiceHandleTaskInProgressChanged += OnServiceTaskInProgressChanged; // task in progress (update services, manage task status)
            m_servicesHandler.ServiceManagerErrorOccured += OnServiceManagerError;
        }

        /// <summary>
        /// Run selected service
        /// </summary>
        private RunCommand m_runCommand;
        public RunCommand RunCommand {
            get {
                if (m_runCommand == null) {
                    m_runCommand = new RunCommand();
                }
                return m_runCommand;
            }
        }

        /// <summary>
        /// Pause selected service
        /// </summary>
        private PauseCommand m_pauseCommand;
        public PauseCommand PauseCommand {
            get {
                if (m_pauseCommand == null) {
                    m_pauseCommand = new PauseCommand();
                }
                return m_pauseCommand;
            }
        }

        /// <summary>
        /// Stop selected service
        /// </summary>
        private StopCommand m_stopCommand;
        public StopCommand StopCommand {
            get {
                if (m_stopCommand == null) {
                    m_stopCommand = new StopCommand();
                }
                return m_stopCommand;
            }
        }

        /// <summary>
        /// Event from ServicesManager: changes in list of services (added/removed service, changed status or other details)
        /// Update list of services in UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServicesChanged(object sender, ChangedServicesEventArgs e) {
            Services = e.Services;
            ProgressBarVisibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Event from ServicesManager: task is in progress
        /// Show progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Information about task: description and status</param>
        private void OnServiceTaskInProgressChanged(object sender, ServiceHandleTaskInProgressArgs e) {
            ProgressBarVisibility = e.IsInProgress ? Visibility.Visible : Visibility.Collapsed;
            ProgressBarText = e.TaskDescription;

            // Block possibility to execute commands if other task in progress
            // Task can be executed in paralalel but it destroys current consistent of UI
            RunCommand.BlockCanExecute = e.IsInProgress;
            PauseCommand.BlockCanExecute = e.IsInProgress;
            StopCommand.BlockCanExecute = e.IsInProgress;
        }

        /// <summary>
        /// Show error message from ServiceManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceManagerError(object sender, ServiceManagerErrorArgs e) {
            ErrorPanelVisibility = Visibility.Visible;
            ExecutionErrorMessage = e.ErrorMessage;
        }

        /// <summary>
        /// List of services, binding with DataGrid
        /// </summary>
        private ObservableCollection<ServiceDescModel> m_services;
        public ObservableCollection<ServiceDescModel> Services {
            get => m_services;
            set {
                m_services = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Selected service in DataGrid
        /// </summary>
        private ServiceDescModel m_selectedItem;
        public ServiceDescModel SelectedItem {
            get => m_selectedItem;
            set {
                m_selectedItem = value;
                OnPropertyChanged();

                // update CanExecute status of commands
                RunCommand.OnChangedSelectedService();
                PauseCommand.OnChangedSelectedService();
                StopCommand.OnChangedSelectedService();
                // update service details information
                ServiceDetailsPanelVisibility = value == null ? Visibility.Collapsed : Visibility.Visible;
                NoServiceDetailsPanelVisibility = value == null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Progress Bar Visibility depends on task execution status. See OnServiceTaskInProgressChanged
        /// </summary>
        private Visibility m_progressBarVisibility;
        public Visibility ProgressBarVisibility {
            get => m_progressBarVisibility;
            set {
                m_progressBarVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Progress Bar Text depends on task execution status. See OnServiceTaskInProgressChanged
        /// </summary>
        private string m_progressBarText = "Getting services...";
        public string ProgressBarText {
            get => m_progressBarText;
            set {
                m_progressBarText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Panel visible when there is selected service
        /// </summary>
        private Visibility m_serviceDetailsPanelVisibility;
        public Visibility ServiceDetailsPanelVisibility {
            get => m_serviceDetailsPanelVisibility;
            set {
                m_serviceDetailsPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Panel visible when there is no selected service
        /// </summary>
        private Visibility m_noServiceDetailsPanelVisibility;
        public Visibility NoServiceDetailsPanelVisibility {
            get => m_noServiceDetailsPanelVisibility;
            set {
                m_noServiceDetailsPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Show/hide error information
        /// </summary>
        private Visibility m_errorPanelVisibility = Visibility.Collapsed;
        public Visibility ErrorPanelVisibility {
            get => m_errorPanelVisibility;
            set {
                m_errorPanelVisibility = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Error message for error panel
        /// </summary>
        private string m_executionErrorMessage;
        public string ExecutionErrorMessage {
            get => m_executionErrorMessage;
            set {
                m_executionErrorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Implementation for INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
