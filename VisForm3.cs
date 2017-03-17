using System;
using System.Windows.Forms;
using System.IO.Ports;
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
        About about = new About();
        //string _bitTxt;   //WARNING: is assigned but its value is never used
        #endregion

        #region Decoder Thread
        bool _doDisplay; // = false;
                         //bool _doDecode = true;

        //Thread _decoderThr; //WARNING: never used
        //string _msg = ""; //WARNING: is never assigned to, and will always have its default value null

        // MethodInvoker _AddTextInvoker;
        #endregion

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
            //while(!ConfigCommunication());

            

            m_DispTimer.Interval = Disp;
            //m_DispTimer.Enabled = true;
            _decodeTimer.Interval = Thread;
            //_decodeTimer.Enabled = true;

            CreateOnlineCurveWin();
            CreateVertWin();

            //_pnf = new PianoForm(_ph.binWr);
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

			CloseCommunication();
            base.OnFormClosing(e);
        }

        void OnAcqOnOffMenue(object sender, EventArgs e) //Toggle Acq. On/Off
        {
            if (_communicationIsOpen)
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
                    try
                    {
                        _ph.SwitchAcq(false);
                        _ph.Flush();
                    }
                    catch (NullReferenceException)
                    {
                        Console.Out.WriteLine("NullReferenceException caught!");
                    }

                    System.Threading.Thread.Sleep(200);

                    // m_SerPort.DiscardInBuffer();
                    // m_DispTimer.Enabled = false;
                    // stw.Stop();
                }
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

        void OnDispTimer(object sender, EventArgs e)
        {
            /* if (ph.CheckValsPerSecond())
            {
              string txt = string.Format("VPS: {0:F1}", ph.valsPerSec);
              PrintMsg(txt);
            } */
            // if( ph.ParseAllPackets() )
            // DisplayValues();

            if (_doDisplay)
            {
                DisplayValues();
                _doDisplay = false;
            }
        }

        void OnDecodeTimer(object sender, EventArgs e)
        {
            if (_ph.ParseAllPackets())
                _doDisplay = true;
        }

        /*void DecoderThreadLoop()
        {
            while (_doDecode)
            {
                Thread.Sleep(T_THREAD);
                if (ph.ParseAllPackets())
                    _doDisplay = true;
            }
            _doDisplay = false;
            ph.SwitchAcq(false);
            ph.Flush();
        }*/

        void DisplayValues()
        {
            for (int i = 0; i < _ph.NVals; i++)
            {
                /* if (i == 8)
                  DisplayLineBits();
                else */
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
            }
        }

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
            if (curveWinMenuItem.Checked)
                _ow.Show(); //Show Curve window

            else
                _ow.Hide(); //Hide Curve window
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
            //TODO: DIalog verbessern SSSetValue
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
            else
            { }
            sssv.Dispose();

        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //TODO: Reset Method
        }

		private void connectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (connectToolStripMenuItem.Checked)
			{
				if (ConfigCommunication())
				{
					connectToolStripMenuItem.Checked = true;
					for (var i = 0; i < 10; i++)
					{
						_ph.ivs[i] = _ivsBuffer[i];
					}
					m_DispTimer.Enabled = true;
					_decodeTimer.Enabled = true;
					_cmp = new CommandParser(_ph.binWr);
					_pnf = new PianoForm(_ph.binWr);
				}
				else
				{
					connectToolStripMenuItem.Checked = false;
					m_DispTimer.Enabled = false;
					_decodeTimer.Enabled = false;
					_cmp = null;
					_pnf = null;
				}
			}
			else
			{
				_cmp = null;
				_pnf = null;
				_decodeTimer.Enabled = false;
				m_DispTimer.Enabled = false;			
				CloseCommunication();			
			}
		}

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (aboutToolStripMenuItem.Checked)
                about.Show(); //Show Curve window

            else
                about.Hide(); //Hide Curve window
        }
    }
}