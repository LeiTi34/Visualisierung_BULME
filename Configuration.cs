
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO.Ports;
using System.IO;
// using System.Diagnostics;
using ZedHL;

namespace vis1
{
  partial class VisForm3
  {
    #region Timing
    const double F_SAMPLE = 100;
    const double T_SAMPLE = 1 / F_SAMPLE;
    const int T_DISP = 100;  // milliSec
    const int T_THREAD = 20; // milliSec
    #endregion

    void ConfigCommunication()
    {
      m_SerPort = new SerialPort("COM3", 115200, Parity.None, 8, StopBits.One);
      // m_SerPort = new SerialPort("COM15", 200000, Parity.None, 8, StopBits.One);
      m_SerPort.ReadBufferSize = 20 * 1024;
      m_SerPort.Open();
      // ph = new SvIdProtocolHandler(m_SerPort, this);
      ph = new SvIdProtocolHandler3(m_SerPort, this);
      ph._scal = Scaling.None; // MaxI16 = +/-1.0
      // ph = new HPerfProtocolHandler(m_SerPort, this);
    }

    void CreateOnlineCurveWin()
    {
      _ow = new OnlineCurveWin3();
      _olc = _ow.olc;
      _olc.buffSize = (int)(20 * F_SAMPLE);
      _olc.SetY1Scale(false, -200, 200);
      _olc.SetY2Scale(false, -1000, 1000);
      _olc.SetXScale(false, 10, 21); // 9.0-10.1  5-11
      
      ph.ivs[0] = _olc.SetCurve2(0, "S1", Color.Red, false, T_SAMPLE);
      ph.ivs[1] = _olc.SetCurve2(1, "S2", Color.Blue, false, T_SAMPLE);
      ph.ivs[2] = _olc.SetCurve2(2, "S3", Color.Green, false, T_SAMPLE);
      // ph.ivs[3] = _olc.SetCurve2(3, "S4", Color.Orange, false, T_SAMPLE);
      // ph.ivs[4] = _olc.SetCurve2(4, "S5", Color.Pink, false, T_SAMPLE);
      
      // ph.ivs[1] = _olc.SetCurve2(1, "s2", Color.Blue, false, T_SAMPLE);
      // ph.ivs[2] = _olc.SetCurve2(2, "s3", Color.Orange, false, T_SAMPLE);
      // ph.ivs[3] = _olc.SetCurve2(3, "s4", Color.Pink, false, T_SAMPLE);
      // ph.ivs[4] = _olc.SetCurve2(4, "s4", Color.Pink, false, T_SAMPLE);
      
      _olc.AxisChange();
      _olc.AddKeyEventHandler(new KeyEventHandler(OnKeyDownOnGraph));
    }
    
    void CreateVertWin()
    {
      _vbw = new VertBarWin();
      string[] names = { "1", "2", "3", "4", "5", "6" };
      _vbw.CreateBars2(names);
      _vbw.SetY1Scale(false, -10, 800);
      _vbw.AxisChange();
    }

    void SetupSliders()
    {
      _sb.ms[0].SetRange(0, 1, 0.01); _sb.ms[0].cb = this;
      _sb.ms[0].Text = "Filt F";
      _sb.ms[1].SetRange(800, 1800, 1); _sb.ms[1].cb = this;
      _sb.ms[1].Text = "Right";
      _sb.ms[2].SetRange(800, 1800, 1); _sb.ms[2].cb = this;
      _sb.ms[2].Text = "Back";
      _sb.ms[3].SetRange(800, 1800, 1); _sb.ms[3].cb = this;
      _sb.ms[3].Text = "Left";
      /* _sb.ms[2].SetRange(0.1, 20.0, 0.1); _sb.ms[2].cb = this;
      _sb.ms[2].Text = "Mod Frequ.";
      _sb.ms[3].SetRange(0.1, 20.0, 0.1); _sb.ms[3].cb = this;
      _sb.ms[3].Text = "ModDev"; */
    }

    public void OnValChanged(int aId, MSlider aSlider) // SliderCB
    {
      if (aId == 0)
      {
        // ph.binWr.WriteSv16((byte)2, (short)aSlider.val);
        ph.binWr.Write((byte)6);
        ph.binWr.Write((float)aSlider.val);
      } 
      /* aId += 2;
      if (aId >= 2 && aId <= 5)
        ph.binWr.WriteSv16((byte)aId, (short)aSlider.val); */
      /* if (aId == 0) {
        ph.binWr.Write((byte)2);
        ph.binWr.Write((float)aSlider.val);
      } */
      ph.binWr.Flush();
    }

    public void OnMoseUp(int aId, MSlider aSlider) // SliderCB
    {
    }
  }
}
