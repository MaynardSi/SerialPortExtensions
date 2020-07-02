using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SickODValueHelper.Utils;
using System.IO.Ports;

namespace SickODValueHelper
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

            var serialPortConfig = ConfigurationsUtils.GetDeviceSerialPortConfiguration("SickODValue");
            SerialPort serialPort = new SerialPort()
            {
                PortName = serialPortConfig["PortName"],
                BaudRate = int.Parse(serialPortConfig["BaudRate"]),
                Parity = (Parity)Enum.Parse(typeof(Parity), serialPortConfig["Parity"]),
                DataBits = int.Parse(serialPortConfig["DataBits"]),
                StopBits = (StopBits)Enum.Parse(typeof(StopBits), serialPortConfig["StopBits"]),
                Handshake = (Handshake)Enum.Parse(typeof(Handshake), serialPortConfig["Handshake"]),
                ReadTimeout = int.Parse(serialPortConfig["ReadTimeout"]),
                WriteTimeout = int.Parse(serialPortConfig["WriteTimeout"])
            };

            SickODController ODValue = new SickODController(serialPort);

            ODValue.Startup();
            ODValue.PingDevice();
            Log.Information($"Height : {ODValue.ReadHeight()}");
            ODValue.Shutdown();
            Console.ReadLine();
            //ISensorHelper ODValue = new ODValueHelper(args);
            //ODValue.OpenSerialPort();
        }
    }
}