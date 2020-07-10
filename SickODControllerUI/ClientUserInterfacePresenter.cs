using System;
using System.IO.Ports;
using SickODValueHelper;

using Serilog;
using System.Windows.Forms;

namespace SickODControllerUI
{
    public class ClientUserInterfacePresenter
    {
        private SerialPort serialPort;
        private SickODController controller; // TODO: Loose couple. Use Interface.

        public ClientUserInterfacePresenter(ClientUserInterfaceView mainView, SickODController mainController)
        {
            controller = mainController;

            #region Register UI events

            //Register UI Events
            mainView.Startup += StartSerialPort;
            mainView.Shutdown += ShutdownSerialPort;
            mainView.Reset += ResetSerialPort;
            mainView.PingDevice += PingSerialPort;
            mainView.DtoSingleMeasure += DtoSingleMeasure;
            mainView.DtoContinuousMeasureToggle += DtoContinuousMeasureToggle;
            mainView.Q2ContinuousMeasureToggle += Q2ContinuousMeasureToggle;
            mainView.Q2DisplayStatus += Q2DisplayStatus;
            mainView.Q2HiDisplayStatus += Q2HiDisplayStatus;
            mainView.Q2LoDisplayStatus += Q2LoDisplayStatus;
            mainView.Q2SetDistanceToDefault += Q2SetDistanceToDefault;
            mainView.Q2HiSetDistance += Q2HiSetDistance;
            mainView.Q2LoSetDistance += Q2LoSetDistance;
            mainView.AveragingDisplaySetting += AveragingDisplaySetting;
            mainView.AveragingSetSpeedToSlow += AveragingSetSpeedToSlow;
            mainView.AveragingSetSpeedToMedium += AveragingSetSpeedToMedium;
            mainView.AveragingSetSpeedToFast += AveragingSetSpeedToFast;
            mainView.MfOnOffToggle += MfOnOffToggle;
            mainView.MfDisplaySetting += MfDisplaySetting;
            mainView.MfFunctionToLaserOff += MfFunctionToLaserOff;
            mainView.MfFunctionToTrigger += MfFunctionToTrigger;
            mainView.MfFunctionToExternalTeach += MfFunctionToExternalTeach;
            mainView.AlarmDisplaySetting += AlarmDisplaySetting;
            mainView.AlarmSetBehaviorToClamp += AlarmSetBehaviorToClamp;
            mainView.AlarmSetBehaviorToHold += AlarmSetBehaviorToHold;
            mainView.BitRateDisplaySetting += BitRateDisplaySetting;
            mainView.BitRateSet += BitRateSet;
            mainView.SetStartingControlCharacter += SetStartingControlCharacter;
            mainView.SetEndingControlCharacter += SetEndingControlCharacter;
            mainView.WriteControlCharactersToggle += WriteControlCharactersToggle;
            mainView.TrimControlCharactersToggle += TrimControlCharactersToggle;
            mainView.WriteLoggingToggle += WriteLoggingToggle;
            mainView.ReadLoggingToggle += ReadLoggingToggle;

            #endregion Register UI events
        }

        private void StartSerialPort(object sender, SerialPortPropertiesEventArgs e)
        {
            // Common serial port exceptions: TimeoutException, IOException, InvalidOperationException
            Log.Information("Opening Serial Port...");
            try
            {
                serialPort = new SerialPort()
                {
                    PortName = e.Port,
                    BaudRate = e.BaudRate,
                    Parity = e.Parity,
                    DataBits = e.Databits,
                    StopBits = e.Stopbits,
                    Handshake = e.Handshake,
                    ReadTimeout = e.ReadTimeout,
                    WriteTimeout = e.WriteTimeout,
                };

                controller.SerialPort = serialPort;
                controller.Startup();
                controller.PingDevice();
                Log.Information("Serial Port Successfully opened.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error has occured. Serial Port Opening Failed.\n{ex.Message}");
            }
        }

        private void ShutdownSerialPort(object sender, EventArgs e)
        {
            Log.Information("Closing Serial Port...");
            try
            {
                controller.Shutdown();
                Log.Information("Serial Port has been succesfully closed.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error has occured. Serial Port failed to close.\n{ex.Message}");
            }
        }

        private void ResetSerialPort(object sender, EventArgs e)
        {
            try
            {
                controller.Reset();
                Log.Information("Serial Port has been succesfully reset.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error has occured. Serial Port and device reset failed.\n{ex.Message}");
            }
        }

        private void PingSerialPort(object sender, EventArgs e)
        {
            Log.Information("Attempting communication with device...");
            HandleCommandExceptions(() =>
            {
                long delay = controller.PingDevice();
                Log.Information($"Connected to device. Response delay = {delay} ms.");
            });
        }

        private void DtoSingleMeasure(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                double height = controller.ReadHeight();
                Log.Information($"Height is {height} mm");
            });
        }

        private void DtoContinuousMeasureToggle(object sender, bool isActivated)
        {
            HandleCommandExceptions(() =>
            {
                if (isActivated)
                {
                    Log.Information("Starting continuous height read...");
                    controller.StartContinuousReadHeight();
                }
                else
                {
                    Log.Information("Stopping continuous height read...");
                    controller.StopContinuousReadHeight();
                }
            });
        }

        private void Q2ContinuousMeasureToggle(object sender, bool isActivated)
        {
            HandleCommandExceptions(() =>
            {
                if (isActivated)
                {
                    Log.Information("Starting Q2 continuous height read...");
                    controller.StartContinuousQ2Output();
                }
                else
                {
                    Log.Information("Stopping Q2 continuous height read...");
                    controller.StopContinuousQ2Output();
                }
            });
        }

        private void Q2DisplayStatus(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.Q2Status();
                Log.Information($"Q2 status: {status}");
            });
        }

        private void Q2HiDisplayStatus(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.Q2HiStatus();
                Log.Information($"Q2 Hi status: {status}");
            });
        }

        private void Q2LoDisplayStatus(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.Q2LoStatus();
                Log.Information($"Q2 Lo status: {status}");
            });
        }

        private void Q2SetDistanceToDefault(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Q2 to default settings...");
                controller.SetQ2ToDefault();
            });
        }

        private void Q2HiSetDistance(object sender, string distance)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Q2 Hi distance...");
                controller.SetQ2Hi(Convert.ToDouble(distance));
            });
        }

        private void Q2LoSetDistance(object sender, string distance)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Q2 Lo distance...");
                controller.SetQ2Lo(Convert.ToDouble(distance));
            });
        }

        private void AveragingDisplaySetting(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.AveragingSpeedStatus();
                Log.Information($"Averaging setting: {status}");
            });
        }

        private void AveragingSetSpeedToSlow(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Averaging speed to SLOW...");
                controller.SetAveragingSpeed(2);
            });
        }

        private void AveragingSetSpeedToMedium(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Averaging speed to MEDIUM...");
                controller.SetAveragingSpeed(1);
            });
        }

        private void AveragingSetSpeedToFast(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Averaging speed to FAST...");
                controller.SetAveragingSpeed(0);
            });
        }

        private void MfOnOffToggle(object sender, bool isActivated)
        {
            HandleCommandExceptions(() =>
            {
                if (isActivated)
                {
                    Log.Information("Activating Multifunctional Input...");
                    controller.MultifunctionalInputOn();
                }
                else
                {
                    Log.Information("Deactivating Multifunctional Input...");
                    controller.MultifunctionalInputOff();
                }
            });
        }

        private void MfDisplaySetting(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.MultifunctionalInputStatus();
                Log.Information($"Multifunctional Input setting: {status}");
            });
        }

        private void MfFunctionToLaserOff(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting MF function to LASER OFF...");
                controller.SetMFFunction(0);
            });
        }

        private void MfFunctionToTrigger(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting MF function to TRIGGER...");
                controller.SetMFFunction(1);
            });
        }

        private void MfFunctionToExternalTeach(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting MF function to EXTERNAL TEACH...");
                controller.SetMFFunction(2);
            });
        }

        private void AlarmDisplaySetting(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.AlarmStatus();
                Log.Information($"Alarm status: {status}");
            });
        }

        private void AlarmSetBehaviorToClamp(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Alarm Behavior to CLAMP...");
                controller.SetAlarmBehavior(0);
            });
        }

        private void AlarmSetBehaviorToHold(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting Alarm Behavior to HOLD...");
                controller.SetAlarmBehavior(1);
            });
        }

        private void BitRateDisplaySetting(object sender, EventArgs e)
        {
            HandleCommandExceptions(() =>
            {
                string status = controller.BaudRateStatus();
                Log.Information($"Baud Rate: {status}");
            });
        }

        private void BitRateSet(object sender, string bitRate)
        {
            HandleCommandExceptions(() =>
            {
                Log.Information("Setting device baud rate...");
                controller.SetBaudRate(bitRate);
            });
        }

        private void SetEndingControlCharacter(object sender, string e)
        {
            //TODO: Fix bug
            controller.StartingControlCharacter = e;
        }

        private void SetStartingControlCharacter(object sender, string e)
        {
            //TODO: Fix bug
            controller.EndingControlCharacter = e;
        }

        private void WriteControlCharactersToggle(object sender, bool isActivated)
        {
            controller.WriteControlCharacter = !controller.WriteControlCharacter;
        }

        private void TrimControlCharactersToggle(object sender, bool isActivated)
        {
            controller.TrimResponseControlCharacters = !controller.TrimResponseControlCharacters;
        }

        private void WriteLoggingToggle(object sender, bool isActivated)
        {
            controller.WriteLoggingEnabled = !controller.WriteLoggingEnabled;
        }

        private void ReadLoggingToggle(object sender, bool isActivated)
        {
            controller.ReadLoggingEnabled = !controller.ReadLoggingEnabled;
        }

        private static T HandleCommandExceptions<T>(Func<T> fn)
        {
            try
            {
                return fn();
            }
            catch (Exception e)
            {
                Log.Error($"Operation Failed. {e.Message}");
            }
            return default;
        }

        private static void HandleCommandExceptions(Action fn)
        {
            try
            {
                fn();
            }
            catch (Exception e)
            {
                Log.Error($"Operation Failed. {e.Message}");
            }
        }
    }
}