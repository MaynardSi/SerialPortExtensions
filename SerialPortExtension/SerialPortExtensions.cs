using System;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;

namespace SerialPortExtension
{
    /// <summary>
    /// Offers additional methods for the SerialPort class relating to communication, logging
    /// </summary>
    public static class SerialPortExtensions
    {
        /// <summary>
        /// Reads the line from the serial port asynchronously.
        /// </summary>
        /// <param name="_serialPort">The serial port.</param>
        /// <param name="startControlChar">The start control character. Default "\x02".</param>
        /// <param name="endControlChar">The end control character. Default "\x03".</param>
        /// <param name="trimResponse">Removes the start and end control characters from the response. Default "true".</param>
        /// <param name="readLoggingEnabled">Logs the responses sent by the communicating device. Default "true".</param>
        /// <returns> The response string from the communicating device</returns>
        public static async Task<string> ReadLineAsync(this SerialPort _serialPort,
            string startControlChar = "\x02", string endControlChar = "\x03",
            bool trimResponse = false, bool readLoggingEnabled = true)
        {
            byte[] buffer = new byte[1];
            string responseBuffer = string.Empty;
            string response = string.Empty;         // The final response to return

            // Read available stream per byte and convert to char to be
            // added onto the response string. Once a terminating character
            // is found, stop reading and return the response string.
            while (true)
            {
                await _serialPort.BaseStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                responseBuffer += _serialPort.Encoding.GetString(buffer);
                if (responseBuffer.EndsWith(_serialPort.NewLine) || responseBuffer.EndsWith(endControlChar))
                {
                    if (trimResponse)
                    {
                        response = response.Substring(startControlChar.Length, response.Length - (endControlChar.Length + 1));
                    }
                    else
                    {
                        response = responseBuffer;
                    }

                    if (readLoggingEnabled) { Log.Information("Reading... [{response}]", response); }

                    return response;
                }
            }
        }

        /// <summary>
        /// Writes a line to the serial port asynchronously.
        /// </summary>
        /// <param name="_serialPort">The serial port.</param>
        /// <param name="toWrite">String to write.</param>
        /// <param name="startControlChar">The start control character. Default "\x02".</param>
        /// <param name="endControlChar">The end control character. Default "\x03".</param>
        /// <param name="writeLoggingEnabled">Logs the commands to send. Default "true".</param>
        public static async Task WriteLineAsync(this SerialPort _serialPort, string toWrite,
            string startControlChar = "\x02", string endControlChar = "\x03",
            bool writeLoggingEnabled = true)
        {
            if (writeLoggingEnabled) { Log.Information("Writing... [{toWrite}]", toWrite); }

            byte[] writeToByte = _serialPort.Encoding.GetBytes(startControlChar + toWrite + endControlChar);
            await _serialPort.BaseStream.WriteAsync(writeToByte, 0, writeToByte.Length).ConfigureAwait(false);
            await _serialPort.BaseStream.FlushAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a command to the connected serial port and returns its response in string.
        /// </summary>
        /// <param name="command">The command to send to through the port.</param>
        /// <returns>A string response of the given command.</returns>
        public static async Task<string> SendCommandAsync(this SerialPort _serialPort, string command,
            string startControlChar = "\x02", string endControlChar = "\x03", bool writeLoggingEnabled = true,
            bool readLoggingEnabled = true, bool trimResponse = false)
        {
            // Write
            Task writeLineTask = _serialPort.WriteLineAsync(command, startControlChar, endControlChar, writeLoggingEnabled);
            Task writeTimeoutTask = Task.Delay(_serialPort.WriteTimeout);
            await CheckAsyncTimeout(writeLineTask, writeTimeoutTask);

            // Read
            Task<string> readLineTask = _serialPort.ReadLineAsync(startControlChar, endControlChar, trimResponse: trimResponse, readLoggingEnabled: readLoggingEnabled);
            Task readTimeoutTask = Task.Delay(_serialPort.ReadTimeout);
            await CheckAsyncTimeout(readLineTask, readTimeoutTask);

            return await readLineTask;
        }

        /// <summary>
        /// Sends a list of commands to the connected serial port and returns its responses.
        /// </summary>
        /// <param name="commands">The commands. String parameter containing commands separated by a delimiter</param>
        /// <param name="delimiter">The delimiter to use for splitting the input string. Default: '&'</param>
        /// <param name="delay">The delay between sending commands in Milliseconds. Default: 0</param>
        /// <returns></returns>
        public static async Task<List<string>> SendCommandsAsync(this SerialPort _serialPort, string commands,
            char delimiter = '&', string startControlChar = "\x02", string endControlChar = "\x03", int delay = 0,
            bool writeLoggingEnabled = true, bool readLoggingEnabled = true, bool trimResponse = false)
        {
            List<string> responses = new List<string>();
            foreach (string command in commands.Split(delimiter))
            {
                await Task.Delay(delay).ConfigureAwait(false);
                responses.Add(await _serialPort.SendCommandAsync(command, startControlChar, endControlChar,
                    writeLoggingEnabled, readLoggingEnabled, trimResponse).ConfigureAwait(false));
            }
            return responses;
        }

        public static async Task CheckAsyncTimeout(Task _task, Task timeoutTask)
        {
            Task completedReadTask = await Task.WhenAny(_task, timeoutTask);
            if (completedReadTask == timeoutTask)
            {
                throw new TimeoutException();
            }
        }
    }
}