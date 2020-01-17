using System;
using System.IO;
using System.IO.Ports;

namespace SerialPrinter
{
    internal class ConnectionWorker
    {
        private readonly SerialPort _serial;

        private string _port;

        public ConnectionWorker()
        {
            int number = Properties.Default.BaudRate;
            Port = Properties.Default.ComName;
            _serial = new SerialPort(string.IsNullOrEmpty(Port) ? "COM1" : Port, number);
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                Properties.Default.ComName = _port;
                Properties.Default.Save();
            }
        }

        public bool Open()
        {
            if (string.IsNullOrEmpty(Port))
            {
                return false;
            }
            if (_serial.IsOpen)
            {
                try
                {
                    _serial.Close();
                }
                catch
                {
                }
            }

            _serial.PortName = Port;

            try
            {
                _serial.Open();
            }
            catch (IOException e)
            {
                return false;
            }

            return true;
        }

        public void Close()
        {
            if (_serial.IsOpen)
            {
                _serial.Close();
            }
        }

        public void Write(byte[] data)
        {
            if (_serial.IsOpen)
            {
                WriteData(data);
            }
            else
            {
                if (Open())
                {
                    WriteData(data);
                }
            }
        }

        private void WriteData(byte[] data)
        {
            try
            {
                _serial.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                //MessageBox.Show("Ошибка отправки данных в COM-порт:\r\n\r\n" + ex.Message);

                try
                {
                    _serial.Close();
                }
                catch
                {
                    // ignored
                }

                Open();
            }
        }
    }
}
