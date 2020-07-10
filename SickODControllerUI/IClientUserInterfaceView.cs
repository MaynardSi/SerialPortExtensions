using System;

namespace SickODControllerUI
{
    public interface IClientUserInterfaceView
    {
        event EventHandler<SerialPortPropertiesEventArgs> Startup;

        event EventHandler Shutdown;
    }
}