using System;
using Serilog;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SerialPortExtension;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ODValueHelperProject
{
    public class ODValueHelper : SensorHelper
    {
        public ODValueHelper(string[] _args) : base(_args)
        {
        }

        public override void CommandProcess(string command)
        {
            throw new NotImplementedException();
        }

        public override async Task CommandProcessAsync(string command)
        {
            while (true)
            {
                Log.Information("Command To Send: {Command}", command);
                // TODO: CHECK IF INPUT IS VALID
                if (!command.Contains('&'))
                {
                    string response = await _serialPort.SendCommandAsync(command).ConfigureAwait(false);
                    Log.Information("Response received: {Response}", response);
                }
                else
                {
                    List<string> responses;
                    if (command.Contains(' '))
                    {
                        string[] commandsAndDelay = command.Split(new char[] { ' ' }, 2);
                        responses = await _serialPort.SendCommandsAsync(commandsAndDelay[0], delay: Int32.Parse(commandsAndDelay[1])).ConfigureAwait(false);
                    }
                    else
                    {
                        responses = await _serialPort.SendCommandsAsync(command).ConfigureAwait(false);
                    }
                    foreach (string response in responses)
                    {
                        Log.Information("Response received: {response}", response);
                    }
                }
                return;
            }
        }
    }
}