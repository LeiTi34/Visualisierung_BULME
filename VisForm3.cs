using System;
using System.Configuration;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using ZedHL;

namespace vis1
{
    public partial class VisForm3 : Form, IPrintCB, SliderCB
    {
        #region Member Variables
        Label[] m_LblAry = new Label[9];
        SerialPort m_SerPort;
        OnlineCurveWin3 _ow;
        OnlineCurveControl _olc;
        VertBarWin _vbw;
        ProtocolHandler _ph;
        // Stopwatch stw = new Stopwatch();
        CommandParser _cmp;
        PianoForm _pnf;
        //string _bitTxt;   //WARNING: is assigned but its value is never used
        #endregion

        #region Decoder Thread
        bool _doDisplay; // = false;
                         //bool _doDecode = true;

        //Thread _decoderThr; //WARNING: never used
        //string _msg = ""; //WARNING: is never assigned to, and will always have its default value null

        // MethodInvoker _AddTextInvoker;
        #endregion
        private int _displayCounter = 0;

        #region Chart
        //private Chart chart = new Chart();
        private Series[] _series = new Series[10];
        private ChartArea _chartArea = new ChartArea() { Name = "ChartArea" };
        //private int _xAxiscount = 0;
        #endregion

        #region Threads
        private Thread Parse;
        private Thread Draw;
        #endregion

        public delegate void AddDataDelegate(int channel, float value);
        //public AddDataDelegate addDataDel;

        public VisForm3()
        {
            InitializeComponent();

            m_LblAry[0] = m_Disp1;
            m_LblAry[1] = m_Disp2;
            m_LblAry[2] = m_Disp3;
            m_LblAry[3] = m_Disp4;
            m_LblAry[4] = m_Disp5;
            m_LblAry[5] = m_Disp6;
            m_LblAry[6] = m_Disp7;
            m_LblAry[7] = m_Disp8;
            m_LblAry[8] = m_Disp9;

            SetupSliders();   //Sliders Gnerieren



            //_bitTxt = "0 0 0 0 0 0xx";  //WARNING: is assigned but its value is never used
        }

        protected override void OnLoad(EventArgs e)
        {
            ConfigCommunication();

            _cmp = new CommandParser(_ph.binWr);

            _ph.ChannelRead = "";
            _ph.ChannelWrite = "";

            //ChartArea hinzufügen
            chart1.ChartAreas.Add(_chartArea);

            //Spuren Initialisiern
            for (var channel = 0; channel < 10; channel++)
            {
                
                //_ph.ChannelSet[channel] = false;

                _series[channel] = new Series("S" + channel + 1)
                {
                    ChartType = SeriesChartType.Line,
                    ChartArea = "ChartArea",
                    BorderWidth = 2,
                    Legend = "Legend1",
                    Name = ConfigurationManager.AppSettings.Get("S" + channel + 1 + "Name"),
                    //IsXValueIndexed = true
                };
                chart1.Series.Add(_series[channel]);

                
            }


            //Threads erstellen
            Parse = new Thread(new ParameterizedThreadStart(_ph.Parse));    //Liest vom Stream
            Parse.Start();

            Draw = new Thread(new ParameterizedThreadStart(AddToChart));    //Zeichnet den Graph
            Draw.Start();

            //CreateOnlineCurveWin();
            //CreateVertWin();

            _pnf = new PianoForm(_ph.binWr);
            //_AddTextInvoker = this.AddText2ListBox;
            // _decoderThr = new Thread(this.DecoderThreadLoop); _decoderThr.Start();
            base.OnLoad(e);
        }


        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            acqOnOffMenuItem.Checked = false;
            OnAcqOnOffMenue(null, null);

            // _doDecode = false;
            // Thread.Sleep(100);
            // _decoderThr.Join();

            _ph.Close();
            m_SerPort?.Close();
            base.OnFormClosing(e);
        }

        void OnAcqOnOffMenue(object sender, EventArgs e) //Toggle Acq. On/Off
        {
            if (acqOnOffMenuItem.Checked)
            {
                m_SerPort.DiscardInBuffer();
                System.Threading.Thread.Sleep(200);

                _ph.SwitchAcq(true);
                _ph.Flush();    

                // m_DispTimer.Enabled = true;
                // stw.Reset(); stw.Start();
            }
            else
            {
                _ph.SwitchAcq(false);
                _ph.Flush();

                System.Threading.Thread.Sleep(200);
                // m_SerPort.DiscardInBuffer();
                // m_DispTimer.Enabled = false;
                // stw.Stop();
            }
        }

        void OnEmptyReceiveBufferMenue(object sender, EventArgs e)  //Empty ReciveBuffer
        {
            m_SerPort.DiscardInBuffer();
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

        /*void OnDispTimer(object sender, EventArgs e)
        {

            if (_doDisplay)
            {
                DisplayValues();
                _doDisplay = false;
            }
        }*/

        /*void OnDecodeTimer(object sender, EventArgs e)
        {
            _doDisplay = _ph.ParseAllPackets();
        }*/


        private void AddToChart(object stateinfo)
        {
            while (true)
            {
                lock (_ph.ChannelRead)
                {
                    if (_ph.logchannel.Count == 0)
                        Monitor.Wait(_ph.ChannelWrite);

                    if(_ph.logchannel.Count > 0)
                        AddData(_ph.logchannel.Dequeue(), _ph.logfloat.Dequeue());

                    Monitor.PulseAll(_ph.ChannelRead);
                }
                //TODO Dynamische X-Achse
                /*_displayCounter++;
                */
            }
        }

        public void AddData(int channel, float value)
        {
            if (chart1.InvokeRequired)
            {
                AddDataDelegate d = new AddDataDelegate(AddData);
                this.Invoke(d, new object[] {channel, value});
            }
            else
            {
                _series[channel].Points.Add(value);

                if (_chartArea.AxisX.Minimum + 200 < _chartArea.AxisX.Maximum)
                {
                    _series[channel].Points.RemoveAt(0);
                }

                _chartArea.AxisX.Minimum = _series[channel].Points[0].XValue;

                chart1.Invalidate();

            }
        }

        /*for (int i = 0; i < _ph.NVals; i++)
        {

            m_LblAry[i].Text = String.Format("{0:F2}", _ph.vf[i]);

        }
        if (_vbw.Visible)
        {
            for (int i = 0; i < _ph.NVals; i++)
                _vbw.SetBarValue(i, _ph.vf[i]);
            _vbw.InvalidateGraph();
        }
        if (_ow.Visible)
        {
            // _ow.Invalidate();
            // _olc.AxisChange();
            _olc.Invalidate();
        }*/

        /*void DisplayLineBits()
        {
            short val = (short)ph.vf[8];
            if ( val==0 )
              return;
            for (int i = 0; i < 6; i++)
            {
              if (  val & (1 << i)   )
                _bitTxt[2 * i] = '1';
              else
                _bitTxt[2 * i] = '0';
            }
            m_LblAry[8].Text = _bitTxt; 
        }*/

        void ToggleAcq()
        {
            if (acqOnOffMenuItem.Checked)
                acqOnOffMenuItem.Checked = false;
            else
                acqOnOffMenuItem.Checked = true;
            OnAcqOnOffMenue(null, null);
        }

        public void DoPrint(string aTxt)
        {
            // _msg = aTxt;
            // this.Invoke(_AddTextInvoker);
            var MaxLength = 255;

            m_MsgLb.Items.Add(aTxt.Length <= MaxLength ? aTxt : aTxt.Substring(0, MaxLength));  //Schneide String zu wenn > MaxLength

            m_MsgLb.SetSelected(m_MsgLb.Items.Count - 1, true);
            if (m_MsgLb.Items.Count > 255)  //Löscht 1. Zeile wenn maximale Zeilenanzahl von 255 Überschritten wurde
            {
                m_MsgLb.Items.RemoveAt(0);
            }
        }

        /*void AddText2ListBox()
        {
            if (_msg.Length <= 255) //Überprüft die maximale Zeichenlänge pro Zeile von 256
            {
                m_MsgLb.Items.Add(_msg);
            }
            m_MsgLb.SetSelected(m_MsgLb.Items.Count - 1, true);
            if (m_MsgLb.Items.Count > 255)  //Löscht 1. Zeile wenn maximale Zeilenanzahl von 255 Überschritten wurde
            {
                m_MsgLb.Items.RemoveAt(0);
            }
        }*/

        void OnKeyDownOnGraph(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 72)
                ToggleAcq();
        }

        //TODO: Absturz bei send Command, Send Button
        void OnSendEditKeyDown(object sender, KeyEventArgs e)
        {
            MessageBox.Show("Test");
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

        void OnKeyBoardMenue(object sender, EventArgs e)    //Toggle Keyboard Window
        {
            if (keyBoardMenuItem.Checked)
                _pnf.Show();    //Show Keyboard window

            else
                _pnf.Hide();    //Hide Keyboard window
        }

        void OnCurveWinOnOffMenue(object sender, EventArgs e) //Toggle Curve Window
        {
            /*if (curveWinMenuItem.Checked)
                chart.Show();
            //_ow.Show(); //Show Curve window
            //Form1 form = new Form1();
            //form.Show();

            else
                chart.Hide()*/
            //_ow.Hide(); //Hide Curve window
            //_ph.form.Hide();
        }

        void OnBarWinMenue(object sender, EventArgs e)  //Toggle Bar Window
        {
            if (barWinMenuItem.Checked)
                _vbw.Show();    //Show Bar window

            else
                _vbw.Hide();    //Hide Bar window
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
            /*SingleShotSetVals sssv = new SingleShotSetVals(_ph.SingleShotTrigger, _ph.SingleShotChannel);
            sssv.Show();
            _ph.SingleShotChannel = sssv.SingleShotChannel;
            _ph.SingleShotTrigger = sssv.SingleShotTrigger;*/

            SingleShotSetVals sssv = new SingleShotSetVals(_ph.SingleShotTrigger, _ph.SingleShotChannel);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (sssv.ShowDialog(this) == DialogResult.OK)
            {
                // Read the contents of testDialog's TextBox.
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

        }
    }
}