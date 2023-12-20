using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using GoHMI;


namespace WpfIntegration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// For using this code, is strictly needed to add reference to the GOCATOR SDK Dll,
    /// I will not put on my github
    /// </summary>
    public partial class MainWindow : Window
    {
        private MessageHandler _messageHandler = MessageHandler.Instance;
        private MessageHandler.RequestSensorParamList_EventHandler _requestSensorList = null;
        private MessageHandler.SensorExposure_EventHandler _sensorExposure = null;
        private MessageHandler.RequestSensorParamList_EventHandler _requestJobList = null;
        private MessageHandler.ChangeCurrentJob_EventHandler _changeJob = null;
        private MessageHandler.ChangeSelectedSensor_EventHandler _changeSensor = null;
        //exposure mode
        int _exposureMode = GoSDK_Constants.GoExposureMode_Undefined;
        Main gocator { get; set; } = null;
        Sensor _sensor = null;
        volatile List<int> _serialList = new List<int>();
        volatile List<string> _jobList = new List<string>();
        //last serial of the sensor
        private int _lastSerial = 0;

        public MainWindow()
        {
            InitializeComponent();

            gocator = Main.Instance;
            _sensor = gocator.sensor;

            _requestSensorList += _messageHandler.OnRequestSensorList_Receive;
            _sensorExposure += _messageHandler.OnSetSensorExposure_Receive;

            _messageHandler.AnswerRequestedSensorList_Send += _messageHandler_AnswerRequestedSensorList_Send;
            _messageHandler.AnswerRequestedJobList_Send += _messageHandler_AnswerRequestedJobList_Send;

            _messageHandler.GetSensorExposure_Send += _messageHandler_GetSensorExposure_Send;

            _messageHandler.CurrentJobIsChanged_Send += _messageHandler_CurrentJobIsChanged_Send;

            _requestJobList += _messageHandler.OnRequestJobList_Receive;

            _changeSensor += _messageHandler.OnChangeSelectedSensor_Receive;


            _messageHandler.SelectedSensorIsChanged_Send += _messageHandler_SelectedSensorIsChanged_Send;

            _requestSensorList(this, new GoHMI.MessageHandler_Data.SensorListParams_EventArgs(_lastSerial, Types.SensorParamListRequestMessage_What.SensorList));
        }

        private void _messageHandler_SelectedSensorIsChanged_Send(object sender, GoHMI.MessageHandler_Data.SensorSerial_EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _messageHandler_AnswerRequestedJobList_Send(object sender, GoHMI.MessageHandler_Data.SensorListParams_EventArgs e)
        {
            if (e.What == Types.SensorParamListRequestMessage_What.JobList)
            {
                _jobList = e.GetStringList();

                UpdateUI();
            }
        }

        private void _messageHandler_CurrentJobIsChanged_Send(object sender, GoHMI.MessageHandler_Data.SensorParams_EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _messageHandler_GetSensorExposure_Send(object sender, GoHMI.MessageHandler_Data.SensorExposure_EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _messageHandler_RequestSensorExposure_Send(object sender, GoHMI.MessageHandler_Data.SensorParams_EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void _messageHandler_AnswerRequestedSensorList_Send(object sender, GoHMI.MessageHandler_Data.SensorListParams_EventArgs e)
        {
            //DeviceSerialList.Items.Clear();
            _serialList = e.GetIntList();

            UpdateUI();

        }

        public void UpdateUI()
        {

            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                DeviceSerialList.Items.Clear();
                JobList.Items.Clear();

                foreach (int value in _serialList.ToList())
                {
                    DeviceSerialList.Items.Add(value);
                }

                foreach (string value in _jobList.ToList())
                {
                    JobList.Items.Add(value);
                }
            });
        }

        private void ChangeExposure1Btn_Click(object sender, RoutedEventArgs e)
        {

            if (Int32.TryParse(Exposure1Txt.Text, out int result))
            {
                _sensorExposure(this, new GoHMI.MessageHandler_Data.SensorExposure_EventArgs(_lastSerial, GoSDK_Constants.GoExposureMode_Single, new List<double>() { result }));
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _sensor.Stop();
        }

        private void DeviceSerialUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            _requestSensorList(this, new GoHMI.MessageHandler_Data.SensorListParams_EventArgs(_lastSerial, Types.SensorParamListRequestMessage_What.SensorList));
        }

        private void DeviceSerialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DeviceSerialList.Items.Count == 0)
                return;

            _lastSerial = (int)DeviceSerialList.SelectedItem;

            _changeSensor(this, new GoHMI.MessageHandler_Data.SensorSerial_EventArgs(_lastSerial));


        }

        private void JobListBtn_Click(object sender, RoutedEventArgs e)
        {
            _requestJobList(this, new GoHMI.MessageHandler_Data.SensorListParams_EventArgs(0, Types.SensorParamListRequestMessage_What.JobList));
        }

        private void DeviceSerialList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            {
                if (DeviceSerialList.Items.Count == 0 || DeviceSerialList.SelectedItem == null)
                    return;

                _lastSerial = (int)DeviceSerialList.SelectedItem;

                _changeSensor(this, new GoHMI.MessageHandler_Data.SensorSerial_EventArgs(_lastSerial));

            }
        }
    }
}
