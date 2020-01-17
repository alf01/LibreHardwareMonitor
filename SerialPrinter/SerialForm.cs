using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;
using SerialPrinter;

namespace OpenHardwareMonitor.GUI
{
    public partial class SerialForm : Form
    {
        private readonly ISettings _settings;
        private readonly Serial _serial;
        private bool _loaded = false;
        private readonly Action _saveConfiguration;

        public SerialForm(ISettings settings, Serial serial, Action saveConfiguration)
        {
            InitializeComponent();

            _settings = settings;
            _serial = serial;
            _saveConfiguration = saveConfiguration;
        }

        public string Port => (string)cboComPort.SelectedItem;

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SerialForm_Load(object sender, EventArgs e)
        {
            cboMaxTempSource.SelectedIndex = int.Parse(_settings.GetValue(nameof(cboMaxTempSource), "0"));

            LoadComPortNames();

            // Right group.
            nMaxFan.Value = decimal.Parse(_settings.GetValue(nameof(nMaxFan), "100"));
            nMinFan.Value = decimal.Parse(_settings.GetValue(nameof(nMinFan), "20"));
            nMaxTemp.Value = decimal.Parse(_settings.GetValue(nameof(nMaxTemp), "80"));
            nMinTemp.Value = decimal.Parse(_settings.GetValue(nameof(nMinTemp), "20"));

            // Flags
            chkManualFan.Checked = bool.Parse(_settings.GetValue(nameof(chkManualFan), "false"));
            chkManualColor.Checked = bool.Parse(_settings.GetValue(nameof(chkManualColor), "false"));

            // Sliders.
            sldManualFan.Value = int.Parse(_settings.GetValue(nameof(sldManualFan), "50"));
            sldManualColor.Value = int.Parse(_settings.GetValue(nameof(sldManualColor), "500"));
            sldLedBrightness.Value = int.Parse(_settings.GetValue(nameof(sldLedBrightness), "50"));
            sldPlotInterval.Value = int.Parse(_settings.GetValue(nameof(sldPlotInterval), "5"));

            _loaded = true;
        }

        private void LoadComPortNames()
        {
            cboComPort.Items.AddRange(SerialPort.GetPortNames());

            if (cboComPort.Items.Count > 0)
            {
                if (cboComPort.Items.Contains(_serial.Port))
                {
                    cboComPort.SelectedItem = _serial.Port;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_serial.Port))
                    {
                        cboComPort.Items.Add(_serial.Port);
                    }
                    else
                    {
                        cboComPort.SelectedIndex = 0;
                        _serial.Port = (string)cboComPort.SelectedItem;
                    }
                }
            }
        }

        private void SaveAndSend()
        {
            if (!_loaded)
            {
                return;
            }

            _settings.SetValue(nameof(cboMaxTempSource), cboMaxTempSource.SelectedIndex.ToString());
            _serial.Port = (string)cboComPort.SelectedItem;

            // Right group.
            _settings.SetValue(nameof(nMaxFan), ((int)nMaxFan.Value).ToString());
            _settings.SetValue(nameof(nMinFan), ((int)nMinFan.Value).ToString());
            _settings.SetValue(nameof(nMaxTemp), ((int)nMaxTemp.Value).ToString());
            _settings.SetValue(nameof(nMinTemp), ((int)nMinTemp.Value).ToString());

            // Flags
            _settings.SetValue(nameof(chkManualFan), chkManualFan.Checked.ToString());
            _settings.SetValue(nameof(chkManualColor), chkManualColor.Checked.ToString());

            // Sliders.
            _settings.SetValue(nameof(sldManualFan), sldManualFan.Value.ToString());
            _settings.SetValue(nameof(sldManualColor), sldManualColor.Value.ToString());
            _settings.SetValue(nameof(sldLedBrightness), sldLedBrightness.Value.ToString());
            _settings.SetValue(nameof(sldPlotInterval), sldPlotInterval.Value.ToString());
            _saveConfiguration.Invoke();
            //_parent.SaveConfiguration();

            // Push data.
            _serial.Send(false);
        }

        #region Event Handlers

        private void sldManualFan_ValueChanged(object sender, EventArgs e)
        {
            lblManualFanValue.Text = sldManualFan.Value.ToString();
            SaveAndSend();
        }
        private void sldManualColor_ValueChanged(object sender, EventArgs e)
        {
            lblManualColorValue.Text = sldManualColor.Value.ToString();
            SaveAndSend();
        }
        private void sldLedBrightness_ValueChanged(object sender, EventArgs e)
        {
            lblLedBrightnessValue.Text = sldLedBrightness.Value.ToString();
            SaveAndSend();
        }
        private void sldPlotInterval_ValueChanged(object sender, EventArgs e)
        {
            lblPlotIntervalValue.Text = sldPlotInterval.Value.ToString();
            SaveAndSend();
        }
        private void nMaxFan_ValueChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void nMinFan_ValueChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void nMinTemp_ValueChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void nMaxTemp_ValueChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void chkManualFan_CheckedChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void chkManualColor_CheckedChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void portBox_TextChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void cboComPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }
        private void cboMaxTempSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveAndSend();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }

    #endregion

}
