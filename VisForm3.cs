/*!
 * @file VisForm3.cs
 * @author Bulme
 * @brief Beinhaltet Klasse für grafische Oberfläche
 */ 
using System;
using System.Windows.Forms;
using System.IO.Ports;
// using System.Diagnostics;
using ZedHL;

namespace vis1
{
    partial class VisForm3 : Form, IPrintCb, SliderCB
    {
        #region Member Variables

        readonly Label[] _mLblAry = new Label[9];
        SerialPort _mSerPort;   //!< Enthaelt COM-Port
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
        bool _doDisplay = false;
        bool _doDecode = true;

        //Thread _decoderThr; //WARNING: never used
        string _msg = ""; //WARNING: is never assigned to, and will always have its default value null

        MethodInvoker _addTextInvoker;
        #endregion

        public VisForm3()
        {
            InitializeComponent();

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

            //_bitTxt = "0 0 0 0 0 0xx";  //WARNING: is assigned but its value is never used
        }

        protected override void OnLoad(EventArgs e)
        {
            ConfigCommunication();
            _cmp = new CommandParser(_ph.BinWr);

            m_DispTimer.Interval = Disp;
            m_DispTimer.Enabled = true;
            _decodeTimer.Interval = Thread;
            _decodeTimer.Enabled = true;

            CreateOnlineCurveWin();
            CreateVertWin();

            _pnf = new PianoForm(_ph.BinWr);
            _addTextInvoker = AddText2ListBox;
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
            _mSerPort.Close();
            base.OnFormClosing(e);
        }

        void OnAcqOnOffMenue(object sender, EventArgs e) //Toggle Acq. On/Off
        {
            if (acqOnOffMenuItem.Checked)
            {
                _mSerPort.DiscardInBuffer();
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
                System.Threading.Thread.Sleep(Thread);
                if (_ph.ParseAllPackets())
                    _doDisplay = true;
            }
            _doDisplay = false;
            _ph.SwitchAcq(false);
            _ph.Flush();
        }*/

        void DisplayValues()
        {
            for (int i = 0; i < _ph.NVals; i++)
            {
                /* if (i == 8)
                  DisplayLineBits();
                else */
                _mLblAry[i].Text = $@"{_ph.Vf[i]:F2}";
            }
            if (_vbw.Visible)
            {
                for (int i = 0; i < _ph.NVals; i++)
                    _vbw.SetBarValue(i, _ph.Vf[i]);
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
            /* short val = (short)ph.vf[8];
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
            acqOnOffMenuItem.Checked = !acqOnOffMenuItem.Checked;
            OnAcqOnOffMenue(null, null);
        }

        public void DoPrint(string aTxt)
        {
            // _msg = aTxt;
            // this.Invoke(_AddTextInvoker);
            m_MsgLb.Items.Add(aTxt);
            m_MsgLb.SetSelected(m_MsgLb.Items.Count - 1, true);
        }

        void AddText2ListBox()
        {
            m_MsgLb.Items.Add(_msg);
            m_MsgLb.SetSelected(m_MsgLb.Items.Count - 1, true);
        }

        void OnKeyDownOnGraph(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 72)
                ToggleAcq();
        }

        void OnSendEditKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 72)
            {
                m_SendEd.Text = "";
                ToggleAcq();
            }
            if (e.KeyValue != 13) // CR
                return;
            _cmp.ParseAndSend(m_SendEd.Text);
            /* short id, val;
            string[] words = m_SendEd.Text.Split(',');
            id = short.Parse(words[0]);
            val = short.Parse(words[1]);
            ph.binWr.Write((byte)id);
            // m_BinWr.WriteSv16((byte)id, val);
            ph.binWr.Flush(); */
        }
        /*!
         * Toggeln des Keyboard-Winows
         */
        void OnKeyBoardMenue(object sender, EventArgs e)    //Toggle Keyboard Window
        {
            if (keyBoardMenuItem.Checked)
                _pnf.Show();    //Show Keyboard window

            else
                _pnf.Hide();    //Hide Keyboard window
        }

        /*!
         * Toggeln des Curve-Windows
         */
        void OnCurveWinOnOffMenue(object sender, EventArgs e) //Toggle Curve Window
        {
            if (curveWinMenuItem.Checked)
                _ow.Show(); //Show Curve window

            else
                _ow.Hide(); //Hide Curve window
        }

        /*!
         * Toggeln des Bar-Windows
         */
        void OnBarWinMenue(object sender, EventArgs e)  //Toggle Bar Window
        {
            if (barWinMenuItem.Checked)
                _vbw.Show();    //Show Bar window

            else
                _vbw.Hide();    //Hide Bar window
        }
    }
}
