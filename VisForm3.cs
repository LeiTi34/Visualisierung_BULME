using System;
using System.Configuration;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using ZedHL;

namespace vis1
{
    public partial class VisForm3 : Form, IPrintCB, SliderCB
    {
        #region Member Variables
        Label[] _mLblAry = new Label[9];
        SerialPort _mSerPort;
        OnlineCurveWin3 _ow;
        OnlineCurveControl _olc;
        //VertBarWin _vbw;
        ProtocolHandler _ph;
        // Stopwatch stw = new Stopwatch();
        CommandParser _cmp;
        //PianoForm _pnf;
        //string _bitTxt;   //WARNING: is assigned but its value is never used
        #endregion

        #region Chart
        //private Chart chart = new Chart();
        private Series[] _series = new Series[10];
        private ChartArea _chartArea = new ChartArea() { Name = "ChartArea" };
        //private int _xAxiscount = 0;
        #endregion

        #region Threads
        private Thread _parse;
        private Thread _draw;
        public delegate void AddDataDelegate(int channel, float value);
        #endregion

        public VisForm3()
        {
            InitializeComponent();

            //TODO ?
            _mLblAry[0] = m_Disp1;
            _mLblAry[1] = m_Disp2;
            _mLblAry[2] = m_Disp3;
            _mLblAry[3] = m_Disp4;
            _mLblAry[4] = m_Disp5;
            _mLblAry[5] = m_Disp6;
            _mLblAry[6] = m_Disp7;
            _mLblAry[7] = m_Disp8;
            _mLblAry[8] = m_Disp9;

            SetupSliders();   //Sliders Gnerieren
        }

        protected override void OnLoad(EventArgs e)
        {
            ConfigCommunication();

            _cmp = new CommandParser(_ph.binWr);

            SetupLineChart();

            //Threads erstellen
            _parse = new Thread(_ph.Parse);    //Liest vom Stream
            _parse.Start();

            _draw = new Thread(AddToChart);    //Zeichnet den Graph
            _draw.Start();

            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            acqOnOffMenuItem.Checked = false;
            OnAcqOnOffMenue(null, null);

            // _doDecode = false;
            // Thread.Sleep(100);
            // _decoderThr.Join();

            _parse.Abort();
            _draw.Abort();

            _ph.Close();
            _mSerPort?.Close();

            base.OnFormClosing(e);
        }

        void OnAcqOnOffMenue(object sender, EventArgs e) //Toggle Acq. On/Off
        {
            if (acqOnOffMenuItem.Checked)
            {
                _mSerPort.DiscardInBuffer();
                Thread.Sleep(200);

                _ph.SwitchAcq(true);
                _ph.Flush();    

                // m_DispTimer.Enabled = true;
                // stw.Reset(); stw.Start();
            }
            else
            {
                _ph.SwitchAcq(false);
                _ph.Flush();

                Thread.Sleep(200);
                // m_SerPort.DiscardInBuffer();
                // m_DispTimer.Enabled = false;
                // stw.Stop();
            }
        }

        void OnEmptyReceiveBufferMenue(object sender, EventArgs e)  //Empty ReciveBuffer
        {
            _mSerPort.DiscardInBuffer();
        }

        void OnClearMessagesMenue(object sender, EventArgs e)   //Clear Messages
        {
            m_MsgLb.Items.Clear();
        }

        void OnAcqPointsOnOffMenue(object sender, EventArgs e)  //Toggle Acq. Points On/Off
        {
            if (!_ow.Visible)
                return;

            _olc.SetAcqPoints(acqPointMenuItem.Checked);
        }

        private void AddToChart(object stateinfo)       //TODO In klasse Auslagern
        {
            while (true)
            {
                lock (_ph.ChannelRead)
                {
                    if (_ph.Logchannel.Count == 0)
                        Monitor.Wait(_ph.ChannelWrite); //Warten wenn keine Daten verfügbar sind

                    if(_ph.Logchannel.Count > 0)
                        AddData(_ph.Logchannel.Dequeue(), _ph.Logfloat.Dequeue());  //Daten auf den Graphen zeichnen

                    Monitor.PulseAll(_ph.ChannelRead);
                }
                //TODO Dynamische X-Achse
                /*_displayCounter++;
                */
            }
        }

        public void AddData(int channel, float value)
        {
            if (lineChart.InvokeRequired)
            {
                AddDataDelegate d = AddData;
                Invoke(d, new object[] {channel, value});
            }
            //TODO Barchart
            else
            {
                _series[channel].Points.Add(value); //Wert auf Graph zeichnen

                if (_chartArea.AxisX.Minimum + 200 < _chartArea.AxisX.Maximum)  //Dynamische X-Achse
                    _series[channel].Points.RemoveAt(0);
                _chartArea.AxisX.Minimum = _series[channel].Points[0].XValue;   

                lineChart.Invalidate();
            }
        }

        /*void ToggleAcq()
        {
            if (acqOnOffMenuItem.Checked)
                acqOnOffMenuItem.Checked = false;
            else
                acqOnOffMenuItem.Checked = true;
            OnAcqOnOffMenue(null, null);
        }*/

        public void DoPrint(string aTxt)
        {
            var maxLength = 255;

            m_MsgLb.Items.Add(aTxt.Length <= maxLength ? aTxt : aTxt.Substring(0, maxLength));  //Schneide String zu wenn > MaxLength

            m_MsgLb.SetSelected(m_MsgLb.Items.Count - 1, true);
            if (m_MsgLb.Items.Count > 255)  //Löscht 1. Zeile wenn maximale Zeilenanzahl von 255 Überschritten wurde
            {
                m_MsgLb.Items.RemoveAt(0);
            }
        }

        /*void OnKeyDownOnGraph(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 72)
                ToggleAcq();
        }*/

        //TODO: Absturz bei send Command, Send Button
        void OnSendEditKeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show(@"Test");
            /*if (e.KeyValue == 72)
            {
                m_SendEd.Text = "";
                ToggleAcq();
            }
            if (e.KeyValue != 13) // CR
                return;
            _cmp.ParseAndSend(m_SendEd.Text);*/




            /* short id, val;
            string[] words = m_SendEd.Text.Split(',');
            id = short.Parse(words[0]);
            val = short.Parse(words[1]);
            ph.binWr.Write((byte)id);
            // m_BinWr.WriteSv16((byte)id, val);
            ph.binWr.Flush(); */
        }

        private void saveToCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ph.SaveToCsv();
        }

        private void enableToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _ph.SingleShotEnabled = !_ph.SingleShotEnabled;
        }

        private void setValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: Dialog verbessern SSSetValue

            SingleShotSetVals sssv = new SingleShotSetVals(_ph.SingleShotTrigger, _ph.SingleShotChannel);

            if (sssv.ShowDialog(this) == DialogResult.OK)
            {
                _ph.SingleShotChannel = sssv.SingleShotChannel;
                _ph.SingleShotTrigger = sssv.SingleShotTrigger;
            }
            sssv.Dispose();

        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //TODO: Reset Method
        }

        private void m_MsgLb_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TODO ?
        }

    }
}