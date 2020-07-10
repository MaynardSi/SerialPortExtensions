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
    public static class Program
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
            ODValue.StartContinuousReadHeight();
            ODValue.StopContinuousReadHeight();
            Log.Information($"ReadHeight : {ODValue.ReadHeight()}");
            ODValue.StartContinuousQ2Output();
            ODValue.StopContinuousQ2Output();
            Log.Information($"Q2Status : {ODValue.Q2Status()}");
            Log.Information($"Q2HiStatus : {ODValue.Q2HiStatus()}");
            Log.Information($"Q2LoStatus : {ODValue.Q2LoStatus()}");
            ODValue.SetQ2Hi(12.5534);
            ODValue.SetQ2Lo(4.2214);
            ODValue.SetQ2ToDefault();
            Log.Information($"AveragingSpeedStatus : {ODValue.AveragingSpeedStatus()}");
            ODValue.SetAveragingSpeed(0);
            ODValue.SetAveragingSpeed(1);
            ODValue.SetAveragingSpeed(2);
            Log.Information($"MultifunctionalInputStatus : {ODValue.MultifunctionalInputStatus()}");
            ODValue.SetMFFunction(0);
            ODValue.SetMFFunction(1);
            ODValue.SetMFFunction(2);
            Log.Information($"AlarmStatus : {ODValue.AlarmStatus()}");
            ODValue.SetAlarmBehavior(0);
            ODValue.SetAlarmBehavior(1);
            ODValue.ResetSettingsToDefault();
            Log.Information($"BaudRateStatus : {ODValue.BaudRateStatus()}");
            ODValue.SetBaudRate("41000");
            ODValue.Shutdown();
            Console.ReadLine();
            //ISensorHelper ODValue = new ODValueHelper(args);
            //ODValue.OpenSerialPort();
        }
    }
}