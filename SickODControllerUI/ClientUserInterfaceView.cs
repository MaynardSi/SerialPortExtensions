using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SickODControllerUI.Utils;

namespace SickODControllerUI
{
    public partial class ClientUserInterfaceView : Form, IClientUserInterfaceView
    {
        public StringWriter messages;

        public ClientUserInterfaceView()

        {
            InitializeComponent();
            initializeComboBoxes();
            SetClientUserInterfaceControls(false);

            // Writing to console on text box
            TextBoxWriter writer = new TextBoxWriter(logRichTextBox);
            Console.SetOut(writer);
            Console.WriteLine("Started");
        }

        #region Event delegates

        public event EventHandler<SerialPortPropertiesEventArgs> Startup;

        public event EventHandler Shutdown;

        public event EventHandler Reset;

        public event EventHandler PingDevice;

        public event EventHandler DtoSingleMeasure;

        public event EventHandler<bool> DtoContinuousMeasureToggle;

        public event EventHandler<bool> Q2ContinuousMeasureToggle;

        public event EventHandler Q2DisplayStatus;

        public event EventHandler Q2HiDisplayStatus;

        public event EventHandler Q2LoDisplayStatus;

        public event EventHandler Q2SetDistanceToDefault;

        public event EventHandler<string> Q2HiSetDistance;

        public event EventHandler<string> Q2LoSetDistance;

        public event EventHandler AveragingDisplaySetting;

        public event EventHandler AveragingSetSpeedToSlow;

        public event EventHandler AveragingSetSpeedToMedium;

        public event EventHandler AveragingSetSpeedToFast;

        public event EventHandler<bool> MfOnOffToggle;

        public event EventHandler MfDisplaySetting;

        public event EventHandler MfFunctionToLaserOff;

        public event EventHandler MfFunctionToTrigger;

        public event EventHandler MfFunctionToExternalTeach;

        public event EventHandler AlarmDisplaySetting;

        public event EventHandler AlarmSetBehaviorToClamp;

        public event EventHandler AlarmSetBehaviorToHold;

        public event EventHandler BitRateDisplaySetting;

        public event EventHandler<string> BitRateSet;

        public event EventHandler<string> SetStartingControlCharacter;

        public event EventHandler<string> SetEndingControlCharacter;

        public event EventHandler<bool> WriteControlCharactersToggle;

        public event EventHandler<bool> TrimControlCharactersToggle;

        public event EventHandler<bool> WriteLoggingToggle;

        public event EventHandler<bool> ReadLoggingToggle;

        #endregion Event delegates

        private void SetClientUserInterfaceControls(bool isEnabled)
        {
            clientControlsFlowLayoutPanel.Enabled = isEnabled;
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
            stopBitsComboBox.SelectedIndex = 1;
        }

        /// <summary>
        /// Method to return action to UI context
        /// </summary>
        /// <param name="action"></param>
        private void InvokeUI(Action action)
        {
            this.Invoke(action);
        }

        private void ToggleText(Control control)
        {
            InvokeUI(() =>
            {
                if (control.Text.Contains("Start"))
                    control.Text = control.Text.Replace("Start", "Stop");
                else if (control.Text.Contains("Stop"))
                    control.Text = control.Text.Replace("Stop", "Start");
                else if (control.Text.Contains("Activate"))
                    control.Text = control.Text.Replace("Activate", "Deactivate");
                else if (control.Text.Contains("Deactivate"))
                    control.Text = control.Text.Replace("Deactivate", "Activate");
            });
        }

        private async void startStopControllerToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            // Before await to avoid cross thread call
            string port = portComboBox.Text;
            int baudRate = Convert.ToInt32(baudRateComboBox.Text);
            Parity parity = (Parity)Enum.Parse(typeof(Parity), parityComboBox.Text);
            int databits = Convert.ToInt32(dataBitsComboBox.Text);
            StopBits stopbits = (StopBits)Enum.Parse(typeof(StopBits), stopBitsComboBox.Text);
            Handshake handshake = (Handshake)Enum.Parse(typeof(Handshake), handshakeComboBox.Text);
            int readTimeout = Convert.ToInt32(readTimeoutTextBox.Text);
            int writeTimeout = Convert.ToInt32(writeTimeoutTextBox.Text);
            await Task.Run(() =>
            {
                if (startStopControllerToggleButton.Checked)
                {
                    InvokeUI(() =>
                    {
                        startStopControllerToggleButton.Text = "Stop";
                        SetClientUserInterfaceControls(true);
                    });
                    Startup?.Invoke(sender, new SerialPortPropertiesEventArgs
                    {
                        Port = port,
                        BaudRate = baudRate,
                        Parity = parity,
                        Databits = databits,
                        Stopbits = stopbits,
                        Handshake = handshake,
                        ReadTimeout = readTimeout,
                        WriteTimeout = writeTimeout
                    });
                }
                else
                {
                    InvokeUI(() =>
                    {
                        startStopControllerToggleButton.Text = "Start";
                        SetClientUserInterfaceControls(false);
                    });
                    Shutdown?.Invoke(sender, e);
                }
            });
        }

        private async void ResetButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Reset?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void PingDeviceButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => PingDevice?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void dtoContinuosMeasureStartStopToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = dtoContinuosMeasureStartStopToggleButton.Checked;
            await Task.Run(() =>
            {
                DtoContinuousMeasureToggle?.Invoke(sender, isChecked);
                ToggleText(dtoContinuosMeasureStartStopToggleButton);
            }).ConfigureAwait(false);
        }

        private async void dtoMeasureButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => DtoSingleMeasure?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void q2ContinuousMeasureStartStopToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = q2ContinuousMeasureStartStopToggleButton.Checked;
            await Task.Run(() =>
            {
                Q2ContinuousMeasureToggle?.Invoke(sender, isChecked);
                ToggleText(q2ContinuousMeasureStartStopToggleButton);
            }).ConfigureAwait(false);
        }

        private async void q2DisplayStatusButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Q2DisplayStatus?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void q2HiDisplayStatusButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Q2HiDisplayStatus?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void q2LoDisplayStatusButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Q2LoDisplayStatus?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void q2SetDistanceToDefaultButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => Q2SetDistanceToDefault?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void q2HiSetDistanceButton_Click(object sender, EventArgs e)
        {
            //TODO: TEST
            string value = "0.0";
            InputBoxValidation validation = (string val) =>
            {
                if (val == "")
                    return "Value cannot be empty.";
                if (!float.TryParse(val, out float v))
                    return "Input must be a valid value.";
                return "";
            };
            await Task.Run(() =>
            {
                if (InputBox.Show("Set Q2 Hi Distance", "Distance", ref value, validation) == DialogResult.OK)
                {
                    Q2HiSetDistance?.Invoke(sender, value);
                }
            }).ConfigureAwait(false);
        }

        private async void q2LoSetDistanceButton_Click(object sender, EventArgs e)
        {
            //TODO: TEST
            string value = "0.0";
            InputBoxValidation validation = (string val) =>
            {
                if (val == "")
                    return "Value cannot be empty.";
                if (!float.TryParse(val, out float v))
                    return "Input must be a valid value.";
                return "";
            };
            await Task.Run(() =>
            {
                if (InputBox.Show("Set Q2 Lo Distance", "Distance", ref value, validation) == DialogResult.OK)
                {
                    Q2LoSetDistance?.Invoke(sender, value);
                }
            }).ConfigureAwait(false);
        }

        private async void averagingDisplaySettingButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AveragingDisplaySetting?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void averagingSetSpeedToSlowButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AveragingSetSpeedToSlow?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void averagingSetSpeedToMediumButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AveragingSetSpeedToMedium?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void averagingSetSpeedToFastButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AveragingSetSpeedToFast?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void mfOnOffToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = mfOnOffToggleButton.Checked;
            await Task.Run(() =>
            {
                MfOnOffToggle?.Invoke(sender, isChecked);
                ToggleText(mfOnOffToggleButton);
            }).ConfigureAwait(false);
        }

        private async void mfDisplaySettingButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MfDisplaySetting?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void mfFunctionToLaserOffButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MfFunctionToLaserOff?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void mfFunctionToTriggerButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MfFunctionToTrigger?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void mfFunctionToExternalTeachButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => MfFunctionToExternalTeach?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void alarmDisplaySettingButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AlarmDisplaySetting?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void alarmSetBehaviorToClampButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AlarmSetBehaviorToClamp?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void alarmSetBehaviorToHoldButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => AlarmSetBehaviorToHold?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void bitRateDisplaySettingButton_Click(object sender, EventArgs e)
        {
            await Task.Run(() => BitRateDisplaySetting?.Invoke(sender, e)).ConfigureAwait(false);
        }

        private async void bitRateSetButton_Click(object sender, EventArgs e)
        {
            string value = "9.6k";
            InputBoxValidation validation = (string val) =>
            {
                string[] validBaud = {"9.6k","19.2k", "38.4k", "57.6k", "76.8k", "115.2k", "128.0k",
                    "230.4k", "256.0k", "312.5k", "460.8k", "625.0k", "250.0k"};
                if (val?.Length == 0)
                    return "Value cannot be empty.";
                if (!validBaud.Contains(val))
                    return "Input must be a valid value.";
                return "";
            };

            await Task.Run(() =>
                {
                    if (InputBox.Show("Set Baud Rate",
                    "Baudrate can be set to: 19.2k, " +
                    "38.4k, 57.6k, 76.8k, 115.2k, 128.0k, " +
                    "230.4k, 256.0k, 312.5k, 460.8k, 625.0k or 250.0k", ref value, validation) == DialogResult.OK)
                    {
                        BitRateSet?.Invoke(sender, value);
                    }
                }).ConfigureAwait(false);
        }

        private async void writeControlCharactersToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            //await Task.Run(() => WriteControlCharacters?.Invoke(sender, e)).ConfigureAwait(false);
            bool isChecked = writeControlCharactersToggleButton.Checked;
            await Task.Run(() =>
            {
                WriteControlCharactersToggle?.Invoke(sender, isChecked);
                ToggleText(writeControlCharactersToggleButton);
            }).ConfigureAwait(false);
        }

        private async void trimControlCharactersToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            //await Task.Run(() => TrimControlCharacters?.Invoke(sender, e)).ConfigureAwait(false);
            bool isChecked = trimControlCharactersToggleButton.Checked;
            await Task.Run(() =>
            {
                TrimControlCharactersToggle?.Invoke(sender, isChecked);
                ToggleText(trimControlCharactersToggleButton);
            }).ConfigureAwait(false);
        }

        private async void writeLoggingToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            //await Task.Run(() => WriteLogging?.Invoke(sender, e)).ConfigureAwait(false);
            bool isChecked = writeLoggingToggleButton.Checked;
            await Task.Run(() =>
            {
                WriteLoggingToggle?.Invoke(sender, isChecked);
                ToggleText(writeLoggingToggleButton);
            }).ConfigureAwait(false);
        }

        private async void readLoggingToggleButton_CheckedChanged(object sender, EventArgs e)
        {
            //await Task.Run(() => ReadLogging?.Invoke(sender, e)).ConfigureAwait(false);
            bool isChecked = readLoggingToggleButton.Checked;
            await Task.Run(() =>
            {
                ReadLoggingToggle?.Invoke(sender, isChecked);
                ToggleText(readLoggingToggleButton);
            }).ConfigureAwait(false);
        }

        private async void setStartControlCharacterButton_Click(object sender, EventArgs e)
        {
            string value = "\\x02";
            await Task.Run(() =>
            {
                if (InputBox.Show("Set Starting Control character [STX]", "Starting Control character [STX]", ref value) == DialogResult.OK)
                {
                    SetStartingControlCharacter?.Invoke(sender, value);
                }
            }).ConfigureAwait(false);
        }

        private async void setEndingControlCharacterButton_Click(object sender, EventArgs e)
        {
            string value = "\\x03";
            await Task.Run(() =>
            {
                if (InputBox.Show("Set Ending Control character [ETX]", "Ending Control character [ETX]", ref value) == DialogResult.OK)
                {
                    SetEndingControlCharacter?.Invoke(sender, value);
                }
            }).ConfigureAwait(false);
        }

        //https://social.technet.microsoft.com/wiki/contents/articles/12347.wpf-howto-add-a-debugoutput-console-to-your-application.aspx
        //http://csharphelper.com/blog/2018/08/redirect-console-window-output-to-a-textbox-in-c/
        public class TextBoxWriter : TextWriter
        {
            // The control where we will write text.
            private RichTextBox MyControl;

            public TextBoxWriter(RichTextBox control)
            {
                MyControl = control;
            }

            public override void Write(string value)
            {
                MyControl.Invoke(new Action(() =>
                    MyControl.AppendText(value)
                ));
            }

            public override Encoding Encoding
            {
                get { return Encoding.Unicode; }
            }
        }
    }
}