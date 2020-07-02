﻿using System;
using Serilog;
using System.Collections.Generic;
using System.IO.Ports;
using SerialPortExtension;
using SickODValueHelper.Utils;
using System.Diagnostics;

namespace SickODValueHelper
{
    public class SickODController : IHeightSensorController
    {
        private const string TAG = "SickODValueController";
        public SerialPort serialPort;
        public SerialPort sp2;

        public SickODController(SerialPort _serialPort)
        {
            serialPort = _serialPort;
        }

        public bool Startup()
        {
            #region sp2 test

            sp2 = new SerialPort()
            {
                PortName = "COM2",
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            sp2.Open();
            sp2.DataReceived += OnDataReceived2;

            #endregion sp2 test

            try
            {
                Log.Information("Opening Serial Port...");
                serialPort.Open();
                Log.Information("Serial Port Successfully opened.");
                PingDevice();
            }
            catch (Exception e)
            {
                Log.Error("Error has occured. Serial Port Opening Failed.");
                Log.Error(e.Message);
                return false;
            }

            return true;
        }

        public bool Shutdown()
        {
            try
            {
                Log.Information("Closing Serial Port...");
                serialPort.Close();
                Log.Information("Serial Port Successfully closed.");
            }
            catch (Exception e)
            {
                Log.Error("Error has occured. Serial Port Closing Failed.");
                Log.Error(e.Message);
                return false;
            }
            return true;
        }

        public bool Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determine connection with device and measure response time.
        /// </summary>
        /// <returns>Bool true on success.</returns>
        public bool PingDevice()
        {
            Log.Information("Attempting communication with device...");
            serialPort.WriteToBuffer("BIT_RATE");

            // Start Timer
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (string.IsNullOrEmpty(serialPort.ReadFromBuffer()))
            {
                return false;
            }
            sw.Stop();

            Log.Information($"Connected to device. Response delay = {sw.ElapsedMilliseconds} ms.");
            return true;
        }

        /// <summary>
        /// Start continual measurement output
        /// </summary>
        public void StartContinuousReadHeight()
        {
            serialPort.SendCommand("START_MEASURE");
            // TODO: Threaded buffered reading
            // TODO: Display each line read
        }

        /// <summary>
        /// Stop continual measurement output.
        /// </summary>
        public void StopContinuousReadHeight()
        {
            serialPort.SendCommand("STOP_MEASURE");
        }

        /// <summary>
        /// Read out measurement once.
        /// </summary>
        /// <returns>Height measurement in millimeters.</returns>
        public double ReadHeight()
        {
            double.TryParse(serialPort.SendCommand("MEASURE"), out double height);
            return height;
        }

        /// <summary>
        /// Start continual Q2 output
        /// </summary>
        public void StartContinuousQ2Output()
        {
            serialPort.SendCommand("START_Q2");
        }

        /// <summary>
        /// Stop continual Q2 output
        /// </summary>
        public void StopContinuousQ2Output()
        {
            serialPort.SendCommand("STOP_Q2");
        }

        /// <summary>
        /// Read out setting of Q2.
        /// </summary>
        /// <returns>Q2 setting in string</returns>
        public string Q2Status()
        {
            return serialPort.SendCommand("Q2");
        }

        /// <summary>
        /// Read out setting of Q2 Hi.
        /// </summary>
        /// <returns>Q2 Hi setting in string.</returns>
        public string Q2HiStatus()
        {
            return serialPort.SendCommand("Q2_Hi");
        }

        /// <summary>
        /// Read out setting of Q2 Lo.
        /// </summary>
        /// <returns>Q2 Lo setting in string.</returns>
        public string Q2LoStatus()
        {
            return serialPort.SendCommand("Q2_Lo");
        }

        /// <summary>
        /// Set Q2 Hi to a trained height.
        /// </summary>
        /// <param name="measure">Q2_HI 60.000 Set Q2 Hi for example to "60 mm"</param>
        public void SetQ2Hi(double measure)
        {
            serialPort.SendCommand($"Q2_HI {measure}");
        }

        /// <summary>
        /// Set Q2 Lo to a trained height.
        /// </summary>
        /// <param name="measure">Q2_LO 40.000 - Set Q2 Lo for example to "40 mm"</param>
        public void SetQ2Lo(double measure)
        {
            serialPort.SendCommand($"Q2_LO {measure}");
        }

        /// <summary>
        /// Set Q2 to default (Health output).
        /// </summary>
        public void SetQ2ToDefault()
        {
            serialPort.SendCommand("Q2_DEFAULT");
        }

        /// <summary>
        /// Read out setting of the speed (Averaging).
        /// </summary>
        /// <returns></returns>
        public string AveragingSpeedStatus()
        {
            return serialPort.SendCommand("AVG");
        }

        /// <summary>
        /// Set Averaging Speed.
        /// </summary>
        /// <param name="speed">Fast: Averaging 1 measurement value e 1 ms (2 ms)
        ///  Medium: Averaging 16 measurement values e 10 ms (15 ms)
        ///  Slow: Averaging 64 measurement values e 35 ms (50 ms)
        ///  </param>
        public void SetAveragingSpeed(string speed)
        {
            serialPort.SendCommand($"AVG {speed}");
        }

        /// <summary>
        /// Read out setting of MF (multifunctional input).
        /// </summary>
        /// <returns>Status of MF as string.</returns>
        public string MultifunctionalInputStatus()
        {
            return serialPort.SendCommand("MF");
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
            switch (function)
            {
                case 0:
                    serialPort.SendCommand("MF SR OFF");
                    break;

                case 1:
                    serialPort.SendCommand("MF SH");
                    break;

                case 2:
                    serialPort.SendCommand("MF TEACH");
                    break;
            }
        }

        /// <summary>
        /// Read out setting for alarm.
        /// </summary>
        /// <returns>Status of Alarm as string.</returns>
        public string AlarmStatus()
        {
            return serialPort.SendCommand("ALARM");
        }

        /// <summary>
        /// Set behavior of Alarm.
        /// 0 - Set behavior during alarm to give out "maximum value" (Clamp)
        /// 1 - Set behavior during alarm to hold last "good measurement value" (Hold)
        /// </summary>
        /// <param name="behavior"></param>
        public void SetAlarmBehavior(int behavior)
        {
            switch (behavior)
            {
                case 0:
                    serialPort.SendCommand("ALARM CLAMP");
                    break;

                case 1:
                    serialPort.SendCommand("ALARM HOLD");
                    break;
            }
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
            serialPort.SendCommand("RESET");
        }

        /// <summary>
        /// Read setting for baud rate.
        /// </summary>
        /// <returns>Baud rate as string.</returns>
        public string BaudRateStatus()
        {
            return serialPort.SendCommand("BIT_RATE");
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
            serialPort.SendCommand($"BIT_RATE {baudRate}");
        }

        #region test

        private void OnDataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            string toWrite = "empty";
            try
            {
                string read = sp2.ReadExisting();
                Console.WriteLine($"SP2 Received: {read}");
                if (read.Contains("BIT_RATE"))
                {
                    toWrite = "\x02" + "9600" + "\x03";
                }
                else if (read.Contains("MEASURE"))
                {
                    toWrite = "\x02" + "14.2432345" + "\x03";
                }

                sp2.Write(toWrite);
                Console.WriteLine($"SP2 Sent: {toWrite}");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Received: READ MESSAGE TIMEOUT...");
            }
        }

        #endregion test
    }
}