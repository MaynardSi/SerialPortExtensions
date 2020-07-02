using System;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;

namespace SerialPortExtension
{
    /// <summary>
    /// Offers additional methods for the SerialPort class relating to communication, logging
    /// </summary>
    public static class SerialPortExtensions
    {
        //TODO: checks for TimeoutException and IOException and InvalidOperationException

        #region Async methods

        /// <summary>
        /// Reads the line from the serial port asynchronously.
        /// </summary>
        /// <param name="_serialPort">The serial port.</param>
        /// <param name="startControlChar">The start control character. Default "\x02".</param>
        /// <param name="endControlChar">The end control character. Default "\x03".</param>
        /// <param name="trimResponseControlChars">Removes the start and end control characters from the response. Default "true".</param>
        /// <param name="readLoggingEnabled">Logs the responses sent by the communicating device. Default "true".</param>
        /// <returns> The response string from the communicating device</returns>
        public static async Task<string> ReadLineAsync(this SerialPort _serialPort,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool trimResponseControlChars = true,
            bool readLoggingEnabled = false)
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
                    if (trimResponseControlChars)
                    {
                        response = responseBuffer.Substring(startControlChar.Length, responseBuffer.Length - (endControlChar.Length + 1));
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
        /// <param name="writeControlChar">Includes control characters to write.</param>
        /// <param name="writeLoggingEnabled">Logs the commands to send. Default "true".</param>
        public static async Task WriteLineAsync(this SerialPort _serialPort,
            string toWrite,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool writeControlChar = true,
            bool writeLoggingEnabled = false
            )
        {
            byte[] writeToByte;
            if (writeLoggingEnabled) { Log.Information("Writing... [{toWrite}]", toWrite); }
            if (writeControlChar)
            {
                writeToByte = _serialPort.Encoding.GetBytes(startControlChar + toWrite + endControlChar);
            }
            else
            {
                writeToByte = _serialPort.Encoding.GetBytes(toWrite);
            }

            await _serialPort.BaseStream.WriteAsync(writeToByte, 0, writeToByte.Length).ConfigureAwait(false);
            await _serialPort.BaseStream.FlushAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a command to the connected serial port and returns its response in string.
        /// </summary>
        /// <param name="command">The command to send to through the port.</param>
        /// <param name="startControlChar">The start control character. Default "\x02".</param>
        /// <param name="endControlChar">The end control character. Default "\x03".</param>
        /// <param name="writeLoggingEnabled">Logs the commands to send. Default "true".</param>
        /// <returns>A response string of the given command.</returns>
        public static async Task<string> SendCommandAsync(this SerialPort _serialPort,
            string command,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool writeControlChar = true,
            bool trimResponseControlChars = true,
            bool writeLoggingEnabled = false,
            bool readLoggingEnabled = false)
        {
            // Write
            Task writeLineTask = _serialPort.WriteLineAsync(command, startControlChar, endControlChar,
                writeControlChar, writeLoggingEnabled);
            await CheckAsyncTimeout(writeLineTask, _serialPort.WriteTimeout).ConfigureAwait(false);

            // Read
            Task<string> readLineTask = _serialPort.ReadLineAsync(startControlChar, endControlChar,
                trimResponseControlChars: trimResponseControlChars, readLoggingEnabled: readLoggingEnabled);
            await CheckAsyncTimeout(readLineTask, _serialPort.ReadTimeout).ConfigureAwait(false);

            return await readLineTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a list of commands to the connected serial port and returns its responses.
        /// </summary>
        /// <param name="commands">The commands. String parameter containing commands separated by a delimiter</param>
        /// <param name="delimiter">The delimiter to use for splitting the input string. Default: '&'</param>
        /// <param name="delay">The delay between sending commands in Milliseconds. Default: 0</param>
        /// <returns></returns>
        public static async Task<List<string>> SendCommandsAsync(this SerialPort _serialPort,
            string commands,
            char delimiter = '&',
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            int delay = 0,
            bool writeLoggingEnabled = false,
            bool readLoggingEnabled = false,
            bool trimResponseControlChars = true)
        {
            List<string> responses = new List<string>();
            foreach (string command in commands.Split(delimiter))
            {
                await Task.Delay(delay).ConfigureAwait(false);
                responses.Add(await _serialPort.SendCommandAsync(command, startControlChar, endControlChar,
                    writeLoggingEnabled, readLoggingEnabled, trimResponseControlChars).ConfigureAwait(false));
            }
            return responses;
        }

        /// <summary>
        /// Throws a TimeoutException when the timeout task completed before the input task
        /// is completed.
        /// </summary>
        /// <param name="_task">The primary task.</param>
        /// <param name="timeoutInMilliseconds">The timeout time in milliseconds.</param>
        private static async Task CheckAsyncTimeout(Task _task, int timeoutInMilliseconds)
        {
            Task timeoutTask = Task.Delay(timeoutInMilliseconds);
            Task completedReadTask = await Task.WhenAny(_task, timeoutTask).ConfigureAwait(false);
            if (completedReadTask == timeoutTask)
            {
                throw new TimeoutException("Task took longer than expected.");
            }
        }

        #endregion Async methods

        #region Sync Methods

        public static string ReadFromBuffer(this SerialPort _serialPort,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool trimResponseControlChars = true,
            bool readLoggingEnabled = false)
        {
            byte[] buffer = new byte[1];
            string responseBuffer = string.Empty;
            string response = string.Empty;         // The final response to return

            // Read available stream per byte and convert to char to be
            // added onto the response string. Once a terminating character
            // is found, stop reading and return the response string.
            while (true)
            {
                _serialPort.Read(buffer, 0, 1);
                responseBuffer += _serialPort.Encoding.GetString(buffer);
                if (responseBuffer.EndsWith(_serialPort.NewLine) || responseBuffer.EndsWith(endControlChar))
                {
                    if (trimResponseControlChars)
                    {
                        response = responseBuffer.Substring(startControlChar.Length, responseBuffer.Length - (endControlChar.Length + 1));
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

        public static void WriteToBuffer(this SerialPort _serialPort,
            string toWrite,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool writeControlChar = true,
            bool writeLoggingEnabled = false)
        {
            byte[] writeToByte;
            if (writeLoggingEnabled) { Log.Information("Writing... [{toWrite}]", toWrite); }

            if (writeControlChar)
            {
                writeToByte = _serialPort.Encoding.GetBytes(startControlChar + toWrite + endControlChar);
            }
            else
            {
                writeToByte = _serialPort.Encoding.GetBytes(toWrite);
            }
            _serialPort.Write(writeToByte, 0, writeToByte.Length);
        }

        public static string SendCommand(this SerialPort _serialPort,
            string command,
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            bool writeControlChar = true,
            bool trimResponseControlChars = true,
            bool writeLoggingEnabled = false,
            bool readLoggingEnabled = false
            )
        {
            // Write
            HandleSerialPortExceptions(() => _serialPort.WriteToBuffer(command, startControlChar,
                endControlChar, writeControlChar, writeLoggingEnabled), "COMMAND WRITE");

            // Read
            return HandleSerialPortExceptions(() => _serialPort.ReadFromBuffer(startControlChar,
                endControlChar, trimResponseControlChars, readLoggingEnabled), "RESPONSE READ");
        }

        public static async Task<List<string>> SendCommands(this SerialPort _serialPort,
            string commands,
            char delimiter = '&',
            string startControlChar = "\x02",
            string endControlChar = "\x03",
            int delay = 0,
            bool writeLoggingEnabled = false,
            bool readLoggingEnabled = false,
            bool trimResponseControlChars = true)
        {
            List<string> responses = new List<string>();
            foreach (string command in commands.Split(delimiter))
            {
                await Task.Delay(delay).ConfigureAwait(false);
                responses.Add(await _serialPort.SendCommandAsync(command, startControlChar, endControlChar,
                    writeLoggingEnabled, readLoggingEnabled, trimResponseControlChars).ConfigureAwait(false));
            }
            return responses;
        }

        #endregion Sync Methods

        #region Wrapper Methods

        /// <summary>
        /// Try Catch wrapper for common serial port exceptions for methods
        /// that returns a value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fn"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static T HandleSerialPortExceptions<T>(Func<T> fn, string operation)
        {
            try
            {
                return fn();
            }
            catch (TimeoutException e)
            {
                e.Data.Add("OperationError", $"Timeout in operation - {operation}");
                throw;
            }
            catch (IOException e)
            {
                e.Data.Add("OperationError", $"IOException in operation - {operation}");
                throw;
            }
            catch (InvalidOperationException e)
            {
                e.Data.Add("OperationError", $"InvalidOperationException in operation - {operation}");
                throw;
            }
            catch (Exception e)
            {
                e.Data.Add("OperationError", $"Failed operation- {operation}");
                throw;
            }
        }

        /// <summary>
        /// Try Catch wrapper for common serial port exceptions for methods
        /// that does not return a value.
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="operation"></param>
        private static void HandleSerialPortExceptions(Action fn, string operation)
        {
            try
            {
                fn();
            }
            catch (TimeoutException e)
            {
                e.Data.Add("OperationError", $"Timeout in operation - {operation}");
                throw;
            }
            catch (IOException e)
            {
                e.Data.Add("OperationError", $"IOException in operation - {operation}");
                throw;
            }
            catch (InvalidOperationException e)
            {
                e.Data.Add("OperationError", $"InvalidOperationException in operation - {operation}");
                throw;
            }
            catch (Exception e)
            {
                e.Data.Add("OperationError", $"Failed operation- {operation}");
                throw;
            }
        }

        #endregion Wrapper Methods
    }
}