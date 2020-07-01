using System;
using Serilog;
using System.Collections.Generic;
using System.IO.Ports;
using SerialPortExtension;

namespace SickODValueHelper
{
    public class SickODController : IHeightSensorController
    {
        private const string TAG = "SickODValueController";
        public SerialPort serialPort;

        public bool Startup()
        {
            //TODO : This code block might be transfered later on
            string portName = "COM1";
            int baudRate = 9600;
            Parity parity = Parity.None;
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            Handshake handshake = Handshake.None;
            int readTimeout = 5000;
            int writeTimeout = 5000;

            bool trimControlChars = true;

            serialPort = new SerialPort()
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                Handshake = handshake,
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout
            };
            try
            {
                Log.Information("Opening Serial Port...");
                serialPort.Open();
                Log.Information("Serial Port Successfully opened.");
                Log.Information("Attempting communication with device...");
                string sendCommandBitRate = serialPort.SendCommand("BIT_RATE", trimResponseControlChars: trimControlChars);
                if (string.IsNullOrEmpty(sendCommandBitRate))
                {
                    Log.Error("No response from device...");
                    return false;
                }
                Log.Information($"Connected to device! BaudRate is {sendCommandBitRate}.");
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
            if (serialPort.IsOpen)
            {
                Log.Information("Closing Serial Port...");
                try
                {
                    serialPort.Close();
                    Log.Information("Serial Port Successfully closed.");
                }
                catch (Exception e)
                {
                    Log.Error("Error has occured. Serial Port Closing Failed.");
                    Log.Error(e.Message);
                    return false;
                }
            }
            return true;
        }

        public bool Reset()
        {
            throw new NotImplementedException();
        }

        public double ReadHeight()
        {
            throw new NotImplementedException();
        }
    }
}