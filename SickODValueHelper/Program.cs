using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            IHeightSensorController ODValue = new SickODController();
            ODValue.Startup();
            ODValue.Shutdown();
            Console.ReadLine();
            //ISensorHelper ODValue = new ODValueHelper(args);
            //ODValue.OpenSerialPort();
        }
    }
}