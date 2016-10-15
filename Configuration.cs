
//using System;

using System.Configuration;
using System.Drawing;
//using System.Windows.Forms;
//using System.Threading;
using System.IO.Ports;
//using System.IO;
// using System.Diagnostics;
using ZedHL;

namespace vis1
{
    partial class VisForm3
    {
        #region Timing
        private const double F_SAMPLE = 100;
        private const double T_SAMPLE = 1 / F_SAMPLE;
        private const int T_DISP = 100;  // milliSec
        private const int T_THREAD = 20; // milliSec
        #endregion

        //Konfigurationsdialog zur Auswahl eines COM-Ports
        void ConfigCommunication()
        {
            // Get a list of serial port names.
            var ports = SerialPort.GetPortNames();

            // Port names zeilenweise in string scheiben
            System.Text.StringBuilder sbportlist = new System.Text.StringBuilder();
            foreach (var port in ports)
            {
                sbportlist.AppendLine(port.Length >= 3 ? port.Remove(4, port.Length - 4) : port);
            }
            string portlist = sbportlist.ToString();

            //TODO: ListBox statt InputBox
            //Dialog zur abfrage des Ports
            var comport =
                Microsoft.VisualBasic.Interaction.InputBox("Geben Sie eine COM-Schnittstelle an:\n\n" + portlist,
                "COM-Schnittstelle", ports[0].Length >= 3 ? ports[0].Remove(4, ports[0].Length - 4) : ports[0]);

            //Konfiguration der Seriellenn Schnitstelle & Lesebuffer definieren
            m_SerPort = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One) { ReadBufferSize = 20 * 1024 };

            m_SerPort.Open(); //Serielle verbindung öffnen

            // ph = new SvIdProtocolHandler(m_SerPort, this);
            ph = new SvIdProtocolHandler3(m_SerPort, this);
            // ph = new HPerfProtocolHandler(m_SerPort, this);
        }

        ///ph._scal = Scaling.None; // MaxI16 = +/-1.0     //ph._scal does not exist? Scaling.None = default

        //Read Settings Method, not working
        /*static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                var result = appSettings[key] ?? "Not Found";
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                return -1;
            }
        }*/
        void CreateOnlineCurveWin() //Gennerieren des Curve Windows  
        {
            _ow = new OnlineCurveWin3();
            _olc = _ow.olc;
            _olc.buffSize = (int)(20 * F_SAMPLE);

            //Skalierungen
            _olc.SetY1Scale(false, -100, 100);    //Skalierung der liken Y-Achse (Y1)
            _olc.SetY2Scale(false, -1000, 1000);  //Skalierung der rechten Y-Achse (Y2)
//            _olc.SetXScale(false, 10, 21); // 9.0-10.1  5-11  //Skalierung der X-Achse 
            _olc.SetXScale(false, 0, 21); // 9.0-10.1  5-11  //Skalierung der X-Achse 


            //TODO: Auslagern in Config-File
            //        _olc.SetCurve(<ID>, <Na>, <Color>,     <Y2>,         )
            ph.ivs[0] = _olc.SetCurve2(0, "S1", Color.Red,  false, T_SAMPLE); //Zeichnet Wert 1, in rot, in den Graph
            ph.ivs[1] = _olc.SetCurve2(1, "S2", Color.Blue, true, T_SAMPLE); //Zeichnet Wert 2, in blau, in den Graph
           // ph.ivs[2] = _olc.SetCurve2(2, "S3", Color.Green, true, T_SAMPLE); //Zeichnet Wert 2, in grün, in den Graph

            // ph.ivs[3] = _olc.SetCurve2(3, "S4", Color.Orange, false, T_SAMPLE);    //Zeichnet Wert 4, in orange, in den Graph
            // ph.ivs[4] = _olc.SetCurve2(4, "S5", Color.Pink, false, T_SAMPLE);  //Zeichnet Wert 5, in pink, in den Graph

            // ph.ivs[1] = _olc.SetCurve2(1, "S2", Color.Blue, false, T_SAMPLE);
            // ph.ivs[2] = _olc.SetCurve2(2, "S3", Color.Orange, false, T_SAMPLE);
            // ph.ivs[3] = _olc.SetCurve2(3, "S4", Color.Pink, false, T_SAMPLE);
            // ph.ivs[4] = _olc.SetCurve2(4, "S4", Color.Pink, false, T_SAMPLE);

            _olc.AxisChange();
            _olc.AddKeyEventHandler(OnKeyDownOnGraph);
        }

        void CreateVertWin()    //Bar Window generiern
        {
            _vbw = new VertBarWin();
            string[] names = { "1", "2", "3", "4", "5", "6" };    //Namen der Bars definieren
            _vbw.CreateBars2(names);
            _vbw.SetY1Scale(false, -10, 800); //Scale
            _vbw.AxisChange();
        }

        void SetupSliders() //Slider Generieren
        {
            _sb.ms[0].SetRange(0, 1, 0.01); _sb.ms[0].cb = this;      //Slider 1 Range definieren
            _sb.ms[0].Text = "Filt F";    //Slider 1 Name definieren
            _sb.ms[1].SetRange(800, 1800, 1); _sb.ms[1].cb = this;    //Slider 2 Range definieren
            _sb.ms[1].Text = "Right";     //Slider 2 Name definiern
            _sb.ms[2].SetRange(800, 1800, 1); _sb.ms[2].cb = this;    //Slider 3 Range definieren
            _sb.ms[2].Text = "Back";      //Slider 3 Name definieren
            _sb.ms[3].SetRange(800, 1800, 1); _sb.ms[3].cb = this;    //Slider 4 Rangee definieren
            _sb.ms[3].Text = "Left";      //Slider 4 Name definieren

            /* _sb.ms[2].SetRange(0.1, 20.0, 0.1); _sb.ms[2].cb = this;
            _sb.ms[2].Text = "Mod Frequ.";
            _sb.ms[3].SetRange(0.1, 20.0, 0.1); _sb.ms[3].cb = this;
            _sb.ms[3].Text = "ModDev"; */
        }

        public void OnValChanged(int aId, MSlider aSlider) // SliderCB  //Slider Wert senden
        {
            if (aId == 0)
            {
                // ph.binWr.WriteSv16((byte)2, (short)aSlider.val);
                ph.binWr.Write((byte)6);
                ph.binWr.Write((float)aSlider.val); //Slider wert auf COM schreiben
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
