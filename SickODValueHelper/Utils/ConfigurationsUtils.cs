using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SickODValueHelper.Utils
{
    public static class ConfigurationsUtils
    {
        /// <summary>
        /// Returns the serial port configuration of a given device in app.config.
        /// </summary>
        /// <param name="device">The device name.</param>
        /// <returns>Returns collection of string keys and string values for
        /// the SerialPort config group.</returns>
        public static NameValueCollection GetDeviceSerialPortConfiguration(string device)
        {
            if (!(ConfigurationManager.GetSection($"DeviceGroup/SerialPort/{device}")
                is NameValueCollection DeviceSerialPortConfig) || DeviceSerialPortConfig.Count == 0)
            {
                throw new Exception("Device Serial Port Configurations are not defined");
            }
            return DeviceSerialPortConfig;
        }
    }
}