using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace SickODControllerUI
{
    public class ClientUserInterfacePresenter
    {
        private readonly IClientUserInterfaceView _view;
        private SerialPort serialPort;

        public ClientUserInterfacePresenter(IClientUserInterfaceView mainView)
        {
            _view = mainView;

            //Register UI Events
            mainView.ConnectSerialPort += ConnectSerialPort;
            mainView.DisconnectSerialPort += DisconnectSerialPort;
        }

        private void ConnectSerialPort(object sender, SerialPortPropertiesEventArgs e)
        {
            serialPort = new SerialPort()
            {
                PortName = e.Port,
                BaudRate = e.BaudRate,
                Parity = e.Parity,
                DataBits = e.Databits,
                StopBits = e.Stopbits,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            try
            {
                serialPort.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void DisconnectSerialPort(object sender, EventArgs e)
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}