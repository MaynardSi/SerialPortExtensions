using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SickODControllerUI
{
    public class SerialPortPropertiesEventArgs
    {
        public String Port { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int Databits { get; set; }
        public StopBits Stopbits { get; set; }
        public Handshake @Handshake { get; set; }
    }
}