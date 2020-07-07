using System;

namespace SickODControllerUI
{
    public interface IClientUserInterfaceView
    {
        event EventHandler<SerialPortPropertiesEventArgs> ConnectSerialPort;

        event EventHandler DisconnectSerialPort;

        event EventHandler<string> SendMessage;

        void UpdateLog(string message);
    }
}