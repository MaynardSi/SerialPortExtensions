using System;
using Serilog;

namespace ODValueHelperProject
{
    public static class ODValueProgram
    {
        [STAThread]
        private static async System.Threading.Tasks.Task Main(string[] args)
        {
            string input;
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u4}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
            ISensorHelper ODValue = new ODValueHelper(args);
            ODValue.OpenSerialPort();

            do
            {
                Console.WriteLine("Input a Command/s. Format: <COMMAND1>&<COMMAND2>&<...>&<LAST_COMMAND> <DELAY IN MILLISECONDS>");
                input = Console.ReadLine();
                try
                {
                    await ODValue.CommandProcessAsync(input).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            } while (!string.Equals(input, "q", StringComparison.OrdinalIgnoreCase));
            ODValue.CloseSerialPort();
            Console.ReadLine();
        }
    }
}