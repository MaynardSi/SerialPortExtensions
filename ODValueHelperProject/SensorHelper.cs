using System;
using Serilog;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ODValueHelperProject
{
    public abstract class SensorHelper : ISensorHelper
    {
        public SerialPort _serialPort;
        private readonly string[] args;

        protected SensorHelper(string[] _args)
        {
            args = _args;
        }

        public void OpenSerialPort()
        {
            _serialPort = new SerialPort()
            {
                PortName = args[0],
                BaudRate = Int32.Parse(args[1]),
                Parity = (Parity)Enum.Parse(typeof(Parity), args[2], true),
                DataBits = Int32.Parse(args[3]),
                StopBits = (StopBits)Enum.Parse(typeof(StopBits), args[4], true),
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            try
            {
                Log.Information("Opening Serial Port using the Following {serialPortProperties}", args);
                _serialPort.Open();
                Log.Information("Connected: {serialPortProperties}", args);
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        public void CloseSerialPort()
        {
            if (_serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                    Log.Information("Disconnected: {serialPortProperties}", args);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
        }

        public abstract void CommandProcess(string command);

        public abstract Task CommandProcessAsync(string command);
    }
}