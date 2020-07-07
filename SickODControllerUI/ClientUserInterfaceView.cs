using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SickODControllerUI
{
    public partial class ClientUserInterfaceView : Form
    {
        public ClientUserInterfaceView()
        {
            InitializeComponent();
            initializeComboBoxes();
            SetClientUserInterfaceControls(false);
        }

        public event EventHandler<SerialPortPropertiesEventArgs> ConnectSerialPort;

        public event EventHandler DisconnectSerialPort;

        private void SetClientUserInterfaceControls(bool isEnabled)
        {
            clientControlsFlowLayoutPanel.Enabled = isEnabled;
            logRichTextBox.Enabled = isEnabled;
        }

        private void initializeComboBoxes()
        {
            List<string> baudRateList = new List<string>() { "9600" };
            List<string> databitsList = new List<string>() { "4", "5", "6", "7", "8" };
            foreach (string s in SerialPort.GetPortNames())
            {
                portComboBox.Items.Add(s);
            }
            foreach (string s in baudRateList)
            {
                baudRateComboBox.Items.Add(s);
            }
            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                parityComboBox.Items.Add(s);
            }

            foreach (string s in databitsList)
            {
                dataBitsComboBox.Items.Add(s);
            }
            foreach (string s in Enum.GetNames(typeof(StopBits)))
            {
                stopBitsComboBox.Items.Add(s);
            }
            foreach (string s in Enum.GetNames(typeof(Handshake)))
            {
                handshakeComboBox.Items.Add(s);
            }
            handshakeComboBox.SelectedIndex = 0;
            portComboBox.SelectedIndex = 0;
            baudRateComboBox.SelectedIndex = 0;
            parityComboBox.SelectedIndex = 0;
            dataBitsComboBox.SelectedIndex = dataBitsComboBox.Items.Count - 1;
            stopBitsComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Method to return action to UI context
        /// </summary>
        /// <param name="action"></param>
        private void InvokeUI(Action action)
        {
            this.Invoke(action);
        }

        public void UpdateLog(string message)
        {
            InvokeUI(() =>
            {
                logRichTextBox.Text += $"{ message } \n";
                logRichTextBox.Refresh();
            });
        }

        private void startStopControllerToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (startStopControllerToggleButton.Checked)
                {
                    ConnectSerialPort?.Invoke(sender, new SerialPortPropertiesEventArgs
                    {
                        Port = this.portComboBox.Text,
                        BaudRate = Convert.ToInt32(baudRateComboBox.Text),
                        Parity = (Parity)Enum.Parse(typeof(Parity), parityComboBox.Text),
                        Databits = Convert.ToInt32(dataBitsComboBox.Text),
                        Stopbits = (StopBits)Enum.Parse(typeof(StopBits), stopBitsComboBox.Text),
                        @Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshakeComboBox.Text)
                    });
                    InvokeUI(() =>
                    {
                        startStopControllerToggleButton.Text = "Stop";
                        SetClientUserInterfaceControls(true);
                    });
                }
                else
                {
                    DisconnectSerialPort?.Invoke(sender, e);
                    InvokeUI(() =>
                    {
                        startStopControllerToggleButton.Text = "Start";
                        SetClientUserInterfaceControls(false);
                    });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void dtoMeasureButton_Click(object sender, EventArgs e)
        {
        }

        private void dtoContinuosMeasureStartStopToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void q2ContinuousMeasureStartStopToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void q2DisplayStatusButton_Click(object sender, EventArgs e)
        {
        }

        private void q2HiDisplayStatusButton_Click(object sender, EventArgs e)
        {
        }

        private void q2LoDisplayStatusButton_Click(object sender, EventArgs e)
        {
        }

        private void q2SetDistanceToDefaultButton_Click(object sender, EventArgs e)
        {
        }

        private void q2HiSetDistanceButton_Click(object sender, EventArgs e)
        {
        }

        private void q2LoSetDistanceButton_Click(object sender, EventArgs e)
        {
        }

        private void averagingDisplaySettingButton_Click(object sender, EventArgs e)
        {
        }

        private void averagingSetSpeedToSlowButton_Click(object sender, EventArgs e)
        {
        }

        private void averagingSetSpeedToMediumButton_Click(object sender, EventArgs e)
        {
        }

        private void button8_Click(object sender, EventArgs e)
        {
        }

        private void mdOnOffToggleButton_Click(object sender, EventArgs e)
        {
        }

        private void mfDisplaySettingButton_Click(object sender, EventArgs e)
        {
        }

        private void mfFunctionToLaserOffButton_Click(object sender, EventArgs e)
        {
        }

        private void mfFunctionToTriggerButton_Click(object sender, EventArgs e)
        {
        }

        private void mfFunctionToExternalTeachButton_Click(object sender, EventArgs e)
        {
        }

        private void alarmDisplaySettingButton_Click(object sender, EventArgs e)
        {
        }

        private void alarmSetBehaviorToClampButton_Click(object sender, EventArgs e)
        {
        }

        private void alarmSetBehaviorToHoldButton_Click(object sender, EventArgs e)
        {
        }

        private void bitRateDisplaySettingButton_Click(object sender, EventArgs e)
        {
        }

        private void bitRateSetButton_Click(object sender, EventArgs e)
        {
        }

        private void writeControlCharactersToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void trimControlCharactersToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void writeLoggingToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void readLoggingToggleButton_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}