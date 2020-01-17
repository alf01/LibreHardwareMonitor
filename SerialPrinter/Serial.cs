using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibreHardwareMonitor.Hardware;

namespace SerialPrinter
{
    public class Serial
    {
        private Computer _computer;
        private readonly ISettings _settings;
        private ConnectionWorker _connectionWorker;

        public Serial(Computer computer, ISettings settings)
        {
            _computer = computer;
            _settings = settings;
            _connectionWorker = new ConnectionWorker();
        }

        public string Port
        {
            get { return _connectionWorker.Port; }
            set { _connectionWorker.Port = value; }
        }

        private float MaxTemp(Computer computer, HardwareType type, string Name = null)
        {
            var gpus = computer.Hardware.Where(x => x.HardwareType == type).ToArray();

            if (gpus.Any())
            {
                float t = 0;
                foreach (var gpu in gpus)
                {
                    if (!string.IsNullOrEmpty(Name))
                    {
                        var temps = gpu.Sensors.Where(x => x.Name == Name && x.SensorType == SensorType.Temperature).ToArray();

                        if (temps.Count() != 0)
                        {
                            t = temps.Average(temp => temp.Value.Value);
                        }

                    }
                    else
                    {

                        var temps = gpu.Sensors.Where(x => x.SensorType == SensorType.Temperature).ToArray();

                        if (temps.Any())
                        {
                            var temp = temps.Max(x => x.Value.Value);
                            t = Math.Max(temp, t);
                        }

                        foreach (var sh in gpu.SubHardware)
                        {
                            temps = sh.Sensors.Where(x => x.SensorType == SensorType.Temperature).ToArray();
                            if (temps.Any())
                            {
                                var temp = temps.Max(x => x.Value.Value);
                                t = Math.Max(temp, t);
                            }
                        }
                    }


                }

                return t;
            }
            else
            {
                return 0;
            }
        }

        private float UsageInPercent(Computer computer, HardwareType type, string Name)
        {
            int n = 0;
            float p = -1;
            var elements = computer.Hardware.Where(device => device.HardwareType == type).ToArray();

            if (elements.Count() > 0)
            {
                foreach (var hardware in elements)
                {
                    var temps = hardware.Sensors.Where(x => x.Name == Name && x.SensorType == SensorType.Load).ToArray();

                    if (temps.Count() != 0)
                    {
                        n++;
                        p = temps.Average(temp => temp.Value.Value);
                    }
                }
            }

            return n > 0 ? p / n : 0;
        }

        public void Send(bool hardwareOnly)
        {
            if (Enabled)
            {
                var gpuMaxTemp = Math.Max(
                 (int)MaxTemp(_computer, HardwareType.GpuNvidia),
                 (int)MaxTemp(_computer, HardwareType.GpuAmd)
             );
                var gpuMaxUsage = Math.Max(
                  (int)UsageInPercent(_computer, HardwareType.GpuAmd, "GPU Core"),
                  (int)UsageInPercent(_computer, HardwareType.GpuNvidia, "GPU Core")
                );
                var gpuMaxMemory = Math.Max(
                  (int)UsageInPercent(_computer, HardwareType.GpuAmd, "GPU Memory"),
                  (int)UsageInPercent(_computer, HardwareType.GpuNvidia, "GPU Memory")
                );

                List<float> data = new List<float>
                {
                    (int)MaxTemp(_computer, HardwareType.Cpu),
                    gpuMaxTemp,
                    (int)MaxTemp(_computer, HardwareType.Motherboard),
                    (int)MaxTemp(_computer, HardwareType.Storage),
                    (int)UsageInPercent(_computer, HardwareType.Cpu, "CPU Total"),
                    gpuMaxUsage,
                    (int)UsageInPercent(_computer, HardwareType.Memory, "Memory"),
                    gpuMaxMemory



                };

                for (int i = 1; i < 17; i++)
                {
                    data.Add((int)MaxTemp(_computer, HardwareType.Cpu, $"CPU Core #{i}"));
                    data.Add((int)UsageInPercent(_computer, HardwareType.Cpu, $"CPU Core #{i}"));
                }

                if (!hardwareOnly)
                {
                    data.AddRange(new float[]{
                        // Right group.
                        float.Parse(_settings.GetValue("nMaxFan", "100")),
                        float.Parse(_settings.GetValue("nMinFan", "20")),
                        float.Parse(_settings.GetValue("nMaxTemp", "100")),
                        float.Parse(_settings.GetValue("nMinTemp", "10")),

                        // Flags
                        bool.Parse(_settings.GetValue("chkManualFan", "false")) ? 1 : 0,
                        bool.Parse(_settings.GetValue("chkManualColor", "false")) ? 1 : 0,

                        // Sliders.
                        float.Parse(_settings.GetValue("sldManualFan", "50")),
                        float.Parse(_settings.GetValue("sldManualColor", "500")),
                        float.Parse(_settings.GetValue("sldLedBrightness", "50")),
                        float.Parse(_settings.GetValue("sldPlotInterval", "5")),

                        float.Parse(_settings.GetValue("cboMaxTempSource", "0")),
                    });
                };

                string tmp = string.Join(";", data.Select(T => T.ToString()).ToArray());

                _connectionWorker.Write(Encoding.ASCII.GetBytes(tmp));
                _connectionWorker.Write(Encoding.ASCII.GetBytes("E"));

            }
        }

        public bool Enabled { get; set; } = false;
    }
}
