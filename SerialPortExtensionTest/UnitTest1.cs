using System;
using System.IO.Ports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SerialPortExtension
{
    [TestClass]
    public class SerialPortPresenter_DummySerialPort
    {
        private SerialPort sp1;
        private SerialPort sp2;
        private string response = string.Empty;
        private List<string> responses = new List<string>();

        public SerialPortPresenter_DummySerialPort()
        {
            sp1 = new SerialPort()
            {
                PortName = "COM1",
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            sp1.Open();
            sp2 = new SerialPort()
            {
                PortName = "COM2",
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            sp2.Open();
            sp2.DataReceived += OnDataReceived2;
        }

        [TestMethod]
        public void SendCommandTest()
        {
            response = sp1.SendCommand("MEASURE", trimResponseControlChars: true);
            Debug.WriteLine($"Resp: {response}");
            Assert.AreEqual(response, "MEASURE");
        }

        [TestMethod]
        public async Task SendCommandsTest()
        {
            List<string> actualResult = new List<string>()
            {
                "START_MEASURE",
                "STOP_MEASURE",
                "MEASURE",
                "START_Q2",
                "STOP_Q2"
            };
            responses = await sp1.SendCommands("START_MEASURE&STOP_MEASURE&MEASURE&START_Q2&STOP_Q2").ConfigureAwait(false);
            foreach (string response in responses)
            {
                Debug.WriteLine($"Resp: {response}");
            }
            CollectionAssert.AreEqual(responses, actualResult);
        }

        [TestMethod]
        public async Task SendCommandAsyncTest()
        {
            response = await sp1.SendCommandAsync("MEASURE", trimResponseControlChars: true).ConfigureAwait(false);
            Debug.WriteLine($"Resp: {response}");
            Assert.AreEqual(response, "MEASURE");
        }

        [TestMethod]
        public async Task SendCommandsAsyncTest()
        {
            List<string> actualResult = new List<string>()
            {
                "START_MEASURE",
                "STOP_MEASURE",
                "MEASURE",
                "START_Q2",
                "STOP_Q2"
            };
            responses = await sp1.SendCommandsAsync("START_MEASURE&STOP_MEASURE&MEASURE&START_Q2&STOP_Q2").ConfigureAwait(false);
            foreach (string response in responses)
            {
                Debug.WriteLine($"Resp: {response}");
            }
            CollectionAssert.AreEqual(responses, actualResult);
        }

        private void OnDataReceived2(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string read = sp2.ReadExisting();
                Debug.WriteLine($"SP2 Received: {read}");
                sp2.Write($"{read}");
                Debug.WriteLine($"SP2 Sent: {read}");
            }
            catch (TimeoutException)
            {
                Debug.WriteLine("Received: READ MESSAGE TIMEOUT...");
            }
        }

        //private void OnDataReceived1(object sender, SerialDataReceivedEventArgs e)
        //{
        //    try
        //    {
        //        // Check if SerialData recieved are ascii characters
        //        if (e.EventType == SerialData.Chars)
        //        {
        //            _receiveNow.Set();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    Debug.WriteLine($"OnDataReceived: {response}");
        //}
    }
}