using SerialPortExtension;
using Serilog;
using System;
using System.Diagnostics;
using System.IO.Ports;

namespace SickODValueHelper
{
    public class SickODController : IHeightSensorController
    {
        private const string TAG = "SickODValueController";

        public string StartingControlCharacter = "\x02";
        public string EndingControlCharacter = "\x03";
        public bool WriteControlCharacter = true;
        public bool TrimResponseControlCharacters = true;
        public bool WriteLoggingEnabled = true;
        public bool ReadLoggingEnabled = true;

        public SickODController()
        {
            //InitTest();
        }

        #region TEST

        //public SerialPort sp2;

        //public void InitTest()
        //{
        //    sp2 = new SerialPort()
        //    {
        //        PortName = "COM2",
        //        BaudRate = 9600,
        //        Parity = Parity.None,
        //        DataBits = 8,
        //        StopBits = StopBits.One,
        //        Handshake = Handshake.None,
        //        ReadTimeout = 500,
        //        WriteTimeout = 500
        //    };
        //    try
        //    {
        //        sp2.Open();
        //        Log.Information("COM2 OPEN");
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //    sp2.DataReceived += OnDataReceived2;
        //}

        //public void OnDataReceived2(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        string read = sp2.ReadExisting();
        //        Log.Information($"SP2 Received: {read}");
        //        string toWrite = read;
        //        sp2.Write(toWrite);
        //        Log.Information($"SP2 Sent: {toWrite}");
        //    }
        //    catch (TimeoutException)
        //    {
        //        Log.Information("Received: READ MESSAGE TIMEOUT...");
        //    }
        //}

        #endregion TEST

        public SickODController(SerialPort _serialPort)
        {
            SerialPort = _serialPort;
        }

        public SerialPort SerialPort { get; set; }

        // TODO: Rethink this approach
        // Im forced to used boolean because
        // of the interface.
        public bool Startup()
        {
            try
            {
                SerialPort.Open();
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        public bool Shutdown()
        {
            try
            {
                SerialPort.Close();
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            return true;
        }

        public bool Reset()
        {
            try
            {
                //returns false if one is false.
                return Shutdown() && Startup();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Determine connection with device and measure response time.
        /// </summary>
        /// <returns>Elapsed time in milliseconds</returns>
        public long PingDevice()
        {
            // Start Timer
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (string.IsNullOrEmpty(BaudRateStatus()))
            {
                // TODO: Replace with custom exception if needed.
                throw new Exception("Device not responding correctly. Please check the connection with the device.");
            }
            sw.Stop();

            return sw.ElapsedMilliseconds;
        }

        /// <summary>
        /// Start continual measurement output.
        /// </summary>
        public void StartContinuousReadHeight()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("START_MEASURE")));
            // TODO: Threaded buffered reading
            // TODO: Display each line read
        }

        /// <summary>
        /// Stop continual measurement output.
        /// </summary>
        public void StopContinuousReadHeight()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("STOP_MEASURE")));
        }

        /// <summary>
        /// Read out measurement once.
        /// </summary>
        /// <returns>Height measurement in millimeters.</returns>
        public double ReadHeight()
        {
            double.TryParse(
                HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("MEASURE"))),
                out double height
            );
            return height;
        }

        /// <summary>
        /// Start continual Q2 output.
        /// </summary>
        public void StartContinuousQ2Output()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("START_Q2")));
        }

        /// <summary>
        /// Stop continual Q2 output.
        /// </summary>
        public void StopContinuousQ2Output()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("STOP_Q2")));
        }

        /// <summary>
        /// Read out setting of Q2.
        /// </summary>
        /// <returns>Q2 setting in string</returns>
        public string Q2Status()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("Q2")));
        }

        /// <summary>
        /// Read out setting of Q2 Hi.
        /// </summary>
        /// <returns>Q2 Hi setting in string.</returns>
        public string Q2HiStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("Q2_Hi")));
        }

        /// <summary>
        /// Read out setting of Q2 Lo.
        /// </summary>
        /// <returns>Q2 Lo setting in string.</returns>
        public string Q2LoStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("Q2_Lo")));
        }

        /// <summary>
        /// Set Q2 Hi to a trained height.
        /// </summary>
        /// <param name="measure">Q2_HI 60.000 Set Q2 Hi for example to "60 mm"</param>
        public void SetQ2Hi(double measure)
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand($"Q2_HI {measure}")));
        }

        /// <summary>
        /// Set Q2 Lo to a trained height.
        /// </summary>
        /// <param name="measure">Q2_LO 40.000 - Set Q2 Lo for example to "40 mm"</param>
        public void SetQ2Lo(double measure)
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand($"Q2_LO {measure}")));
        }

        /// <summary>
        /// Set Q2 to default (Health output).
        /// </summary>
        public void SetQ2ToDefault()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("Q2_DEFAULT")));
        }

        /// <summary>
        /// Read out setting of the speed (Averaging).
        /// </summary>
        /// <returns></returns>
        public string AveragingSpeedStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("AVG")));
        }

        /// <summary>
        /// Set Averaging Speed.
        /// </summary>
        /// <param name="speed">Fast: Averaging 1 measurement value e 1 ms (2 ms)
        ///  Medium: Averaging 16 measurement values e 10 ms (15 ms)
        ///  Slow: Averaging 64 measurement values e 35 ms (50 ms)
        ///  </param>
        public void SetAveragingSpeed(int speed)
        {
            string command = string.Empty;
            switch (speed)
            {
                case 0:
                    command = "AVG FAST";
                    break;

                case 1:
                    command = "AVG MEDIUM";
                    break;

                case 2:
                    command = "AVG SLOW";
                    break;
            }

            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand(command)));
        }

        /// <summary>
        /// Activate MF
        /// </summary>
        public void MultifunctionalInputOn()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("ON")));
        }

        /// <summary>
        /// Deactivate MF
        /// </summary>
        public void MultifunctionalInputOff()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("ON")));
        }

        /// <summary>
        /// Read out setting of MF (multifunctional input).
        /// </summary>
        /// <returns>Status of MF as string.</returns>
        public string MultifunctionalInputStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("MF")));
        }

        /// <summary>
        /// Set function of MF.
        /// 0 - Set function of MF to "Laser off"
        /// 1 - Set function of MF to "Trigger"
        /// 2 - Set function of MF to "External Teach"
        /// </summary>
        /// <param name="function"></param>
        public void SetMFFunction(int function)
        {
            string command = string.Empty;
            switch (function)
            {
                case 0:
                    command = "MF SR OFF";
                    break;

                case 1:
                    command = "MF SH";
                    break;

                case 2:
                    command = "MF TEACH";
                    break;
            }
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand(command)));
        }

        /// <summary>
        /// Read out setting for alarm.
        /// </summary>
        /// <returns>Status of Alarm as string.</returns>
        public string AlarmStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("ALARM")));
        }

        /// <summary>
        /// Set behavior of Alarm.
        /// 0 - Set behavior during alarm to give out "maximum value" (Clamp)
        /// 1 - Set behavior during alarm to hold last "good measurement value" (Hold)
        /// </summary>
        /// <param name="behavior"></param>
        public void SetAlarmBehavior(int behavior)
        {
            string command = string.Empty;
            switch (behavior)
            {
                case 0:
                    command = "ALARM CLAMP";
                    break;

                case 1:
                    command = "ALARM HOLD";
                    break;
            }
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand(command)));
        }

        /// <summary>
        /// The following parameters are reset to factory settings.
        /// 1. Start of measuring range => 4mA(0V) [analog output model only].
        /// 2. End of measuring range => 20mA(10V) [analog output model only].
        /// 3. Q1 => Complete meaasuring range[except communication model].
        /// 4. Q2 => Health output. / 5. Avg => Medium. / 6. MF => Laser off.
        /// 7. Alarm => Clamp. / 8. Baud rate => 9600bps[Communication model only].
        /// 9. Sampling rate => 500us (250mm or longer type 750us).
        /// </summary>
        public void ResetSettingsToDefault()
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("RESET")));
        }

        /// <summary>
        /// Read setting for baud rate.
        /// </summary>
        /// <returns>Baud rate as string.</returns>
        public string BaudRateStatus()
        {
            return HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand("BIT_RATE")));
        }

        /// <summary>
        /// Set baud rate to "9.6 kBaud“ (Default), baud
        /// rate can be set to: 19.2 kBaud, 38.4 kBaud
        /// 57.6 kBaud, 76.8 kBaud, 115.2 kBaud,
        /// 128.0 kBaud, 230.4 kBaud, 256.0 kBaud,
        /// 312.5 kBaud, 460.8 kBaud, 625.0 kBaud or
        /// 1250.0 kBaud
        /// </summary>
        /// <param name="baudRate"></param>
        public void SetBaudRate(string baudRate)
        {
            HandleCommandExceptions(() => CheckValidDeviceResponse(() => SendCommand($"BIT_RATE {baudRate}")));
        }

        #region Wrapper methods

        private string SendCommand(string command)
        {
            return SerialPort.SendCommand(command,
                startControlChar: StartingControlCharacter,
                endControlChar: EndingControlCharacter,
                writeControlChar: WriteControlCharacter,
                trimResponseControlChars: TrimResponseControlCharacters,
                writeLoggingEnabled: WriteLoggingEnabled,
                readLoggingEnabled: ReadLoggingEnabled
                );
        }

        private static string CheckValidDeviceResponse(Func<string> commandToSend)
        {
            string response = commandToSend();
            if (response.Contains("?"))
            {
                //TODO: Create Custom Exception for device fail
                throw new Exception("Device returned '?' : Failure");
            }
            return response;
        }

        private static T HandleCommandExceptions<T>(Func<T> fn)
        {
            try
            {
                return fn();
            }
            catch (Exception)
            {
                // Do something here
                throw;
            }
        }

        #endregion Wrapper methods
    }
}