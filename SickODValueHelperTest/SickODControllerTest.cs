using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SickODValueHelper;
using SerialPortExtension;
using Serilog;
using System.IO.Ports;
using System.Diagnostics;

namespace SickODValueHelperTest
{
    [TestClass]
    public class SickODControllerTest
    {
        private SerialPort sp2;

        [TestMethod]
        public void TestMethod1()
        {
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

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
            IHeightSensorController ODValue = new SickODController();
            ODValue.Startup();
            ODValue.Shutdown();
        }

        private void OnDataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string read = sp2.ReadExisting();
                Console.WriteLine($"SP2 Received: {read}");
                string toWrite = "\x02" + "9600" + "\x03";
                sp2.Write(toWrite);
                Console.WriteLine($"SP2 Sent: {toWrite}");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Received: READ MESSAGE TIMEOUT...");
            }
        }
    }
}