/*!
 * @file Configuration.cs
 * @author Bulme
 * @brief Konfiguration der Graphen, Kommunikation, Sample frequenz, etc.
 */ 
//using System;
using System.Drawing;
using System.Windows.Forms;
//using System.Threading;
using System.IO.Ports;
//using System.IO;
// using System.Diagnostics;
using ZedHL;

namespace vis1
{
    public partial class VisForm3
    {
        #region Timing
        const double FSample = 100;    //Sample Frequenz
        const double Sample = 1 / FSample;   //Sample Periodendauer
        const int Disp = 100;  // milliSec
        const int Thread = 20; // Êinlesen alle x milliSec
        #endregion

        /*!
         * Konfigurieren des Streams. Öffnet eine Dialog zur eingabe der COM-Schnittstelle.
         * @param m_SerialPort  UART Konfiguration
         * @param ph    SvIdProtocolHandler3 mit eingestellter Konfiguration
         */
        void ConfigCommunication()
        {
            string comport = Microsoft.VisualBasic.Interaction.InputBox("Geben Sie eine COM-Schnittstelle an:", "COM-Schnittstelle", "COM5"); //Auswahldialog der Serieller Schnitstelle
            _mSerPort = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One); //Konfiguration der Seriellenn Schnitstelle

            _mSerPort.ReadBufferSize = 20 * 1024; //Lesebuffer definieren
            _mSerPort.Open(); //Serielle verbindung öffnen

            // ph = new SvIdProtocolHandler(m_SerPort, this);
            _ph = new SvIdProtocolHandler3(_mSerPort, this);
            // ph = new HPerfProtocolHandler(m_SerPort, this);
        }

        //ph._scal = Scaling.None; // MaxI16 = +/-1.0     //ph._scal does not exist? Scaling.None = default
        /*!
         * Generiert das Curve-Fenster
         * 
         */
        void CreateOnlineCurveWin() //Gennerieren des Curve Windows  
        {
            _ow = new OnlineCurveWin3();
            _olc = _ow.olc;
            _olc.buffSize = (int)(20 * FSample);

            //Skalierungen
            _olc.SetY1Scale(false, -200, 300);    //!< Skalierung der liken Y-Achse (Y1)
            _olc.SetY2Scale(false, -1000, 2000);  //!< Skalierung der rechten Y-Achse (Y2)
            _olc.SetXScale(false, 10, 21); // 9.0-10.1  5-11  //!<Skalierung der X-Achse


            //_olc.SetCurve( <Index>, <Name>, <Color>, <Y2>, <?> )
            _ph.Ivs[0] = _olc.SetCurve2(0, "S1", Color.Red, false, Sample);  //!< Zeichnet Wert 1, in rot, in den Graph
            _ph.Ivs[1] = _olc.SetCurve2(1, "S2", Color.Blue, false, Sample); //!< Zeichnet Wert 2, in blau, in den Graph
            _ph.Ivs[2] = _olc.SetCurve2(2, "S3", Color.Green, true, Sample); //!< Zeichnet Wert 2, in grün, in den Graph

            // ph.ivs[3] = _olc.SetCurve2(3, "S4", Color.Orange, false, T_SAMPLE);    //Zeichnet Wert 4, in orange, in den Graph
            // ph.ivs[4] = _olc.SetCurve2(4, "S5", Color.Pink, false, T_SAMPLE);  //Zeichnet Wert 5, in pink, in den Graph

            // ph.ivs[1] = _olc.SetCurve2(1, "s2", Color.Blue, false, T_SAMPLE);
            // ph.ivs[2] = _olc.SetCurve2(2, "s3", Color.Orange, false, T_SAMPLE);
            // ph.ivs[3] = _olc.SetCurve2(3, "s4", Color.Pink, false, T_SAMPLE);
            // ph.ivs[4] = _olc.SetCurve2(4, "s4", Color.Pink, false, T_SAMPLE);

            _olc.AxisChange();
            _olc.AddKeyEventHandler(new KeyEventHandler(OnKeyDownOnGraph));
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

        /*!
         * Schreibt neuen Wert auf Stream wenn sich der Slider aendert und aId 0 ist
         * @param aSlider   Aktueller Slider-Wert
         * @param aId   ID
         */
        public void OnValChanged(int aId, MSlider aSlider) // SliderCB  //Slider Wert senden
        {
            if (aId == 0)
            {
                // ph.binWr.WriteSv16((byte)2, (short)aSlider.val);
                _ph.BinWr.Write((byte)6);
                _ph.BinWr.Write((float)aSlider.val); //Slider wert auf COM schreiben
            }

            /* aId += 2;
            if (aId >= 2 && aId <= 5)
              ph.binWr.WriteSv16((byte)aId, (short)aSlider.val); */
            /* if (aId == 0) {
              ph.binWr.Write((byte)2);
              ph.binWr.Write((float)aSlider.val);
            } */

            _ph.BinWr.Flush();
        }

        public void OnMoseUp(int aId, MSlider aSlider) // SliderCB
        {
        }
    }
}
