using LibreHardwareMonitor.Hardware;

namespace SerialPrinter
{
    public class Serial
    {
        private Computer _computer;
        private ConnectionWorker _connectionWorker;

        public Serial(Computer computer)
        {
            _computer = computer;
            _connectionWorker = new ConnectionWorker();
        }

        public void Send()
        {
            if (Enabled)
            {

            }
        }

        public bool Enabled { get; set; } = false;
    }
}
