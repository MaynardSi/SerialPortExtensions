using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Data;
using System.Threading;
using System.Diagnostics;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

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
        /// <returns></returns>
        public static async Task<string> ReadLineAsync(this SerialPort _serialPort,
            string startControlChar = "\x02", string endControlChar = "\x03")
        {
            byte[] buffer = new byte[1];
            string response = string.Empty;

            // Read available stream per byte and convert to char to be
            // added onto the response string. Once a terminating character
            // is found, stop reading and return the response string.
            while (true)
            {
                await _serialPort.BaseStream.ReadAsync(buffer, 0, 1).ConfigureAwait(false);
                response += _serialPort.Encoding.GetString(buffer);
                Debug.WriteLine($"readLineAsync Resp: {response}");

                if (response.EndsWith(endControlChar))
                {
                    return response.Substring(startControlChar.Length, response.Length - (endControlChar.Length + 1));
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
        public static async Task WriteLineAsync(this SerialPort _serialPort, string toWrite,
            string startControlChar = "\x02", string endControlChar = "\x03")
        {
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
            string startControlChar = "\x02", string endControlChar = "\x03")
        {
            await _serialPort.WriteLineAsync(command, startControlChar, endControlChar).ConfigureAwait(false);
            return await _serialPort.ReadLineAsync(startControlChar, endControlChar).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a list of commands to the connected serial port and returns its responses.
        /// </summary>
        /// <param name="commands">The commands. String parameter containing commands separated by a delimiter</param>
        /// <param name="delimiter">The delimiter to use for splitting the input string. Default: '&'</param>
        /// <param name="delay">The delay between sending commands in Milliseconds. Default: 0</param>
        /// <returns></returns>
        public static async Task<List<string>> SendCommandsAsync(this SerialPort _serialPort, string commands,
            char delimiter = '&', string startControlChar = "\x02", string endControlChar = "\x03", int delay = 0)
        {
            List<string> responses = new List<string>();
            foreach (string command in commands.Split(delimiter))
            {
                await Task.Delay(delay).ConfigureAwait(false);
                responses.Add(await _serialPort.SendCommandAsync(command, startControlChar, endControlChar).ConfigureAwait(false));
            }
            return responses;
        }

        /// <summary>
        /// A generic method for handling timeout exceptions for a given method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn">The function to wrap around a try-catch.</param>
        /// <returns></returns>
        private static T HandleTimeoutException<T>(Func<T> fn)
        {
            T result;
            try
            {
                result = fn();
            }
            catch (TimeoutException)
            {
                throw;
            }
            return result;
        }
    }
}