using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using ZedHL;

namespace vis1
{
    partial class VisForm3
    {
        public int Xmin;
        public int Xmax;
        #region Timing
        private const double FSample = 100;
        private const double Sample = 1 / FSample;
        private const int Disp = 100;  // milliSec
        private const int Thread = 20; // milliSec
        #endregion

        //Konfigurationsdialog zur Auswahl eines COM-Ports
        void ConfigCommunication()
        {
            var comport = "";
            do
            {
                var comDialog = new ChooseCom();

                if (comDialog.ShowDialog(this) == DialogResult.OK)
                {
                    comport = comDialog.Port;
                    comDialog.Dispose();
                }
            } while (comport == "");

            m_SerPort = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One) { ReadBufferSize = 20 * 1024 };

            try
            {
                m_SerPort.Open(); //Serielle verbindung öffnen
            }
            catch (IOException)
            {
                MessageBox.Show(@"IO Exception: " + comport + @" konnte nicht gefunden werden!");
                Environment.Exit(1);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Exception: " + comport + @" konnte nicht gefunden werden!");
                Environment.Exit(1);
            }
            finally
            {
                //statusStrip.Items.Add("COM-Port opened successful");
                _ph = new BufProtocolHandler(m_SerPort, this);
            }

            //_ph = new SvIdProtocolHandler(m_SerPort, this);
            //_ph = new SvIdProtocolHandler3(m_SerPort, this);
            // _ph = new HPerfProtocolHandler(m_SerPort, this);

        }

        ///ph._scal = Scaling.None; // MaxI16 = +/-1.0     //ph._scal does not exist? Scaling.None = default

        void CreateOnlineCurveWin() //Gennerieren des Curve Windows  
        {
            _ow = new OnlineCurveWin3();
            _olc = _ow.olc;
            _olc.buffSize = (int)(20 * FSample);

            // Skalierungen von app.conf file einlesen und setzen
            // Y1-Axis
            _olc.SetY1Scale(false,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y1min")),
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y1max")));
            
            // Y2-Axis
            _olc.SetY2Scale(false,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y2min")),
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y2max")));

            // X-Axis
            _olc.SetXScale(false,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Xmin")),
            Convert.ToInt32(ConfigurationManager.AppSettings.Get("Xmax"))); 

            // olc.SetCurve(<ID>, <Name>, <Color>, <Y2>, )
            for (var track = 1; track <= 10; track++)    //Track 1...5
            {
                if (Convert.ToBoolean(ConfigurationManager.AppSettings.Get("S" + track + "Enable")))    //Enable von app.conf file Lesen
                {
                    //Werte aus app.conf file einlesen und dem richtigen Track zuweisen
                    _ph.ivs[track-1] = _olc.SetCurve2(
                        track-1,  //ID
                        ConfigurationManager.AppSettings.Get("S" + track + "Name"), //Name
                        Color.FromName(ConfigurationManager.AppSettings.Get("S" + track + "Color")),    //Color
                        Convert.ToBoolean(ConfigurationManager.AppSettings.Get("S" + track + "Y2")),    //Y2-Axis
                        Sample );   //Sample-Rate
                }
            }

            _olc.AxisChange();
            _olc.AddKeyEventHandler(OnKeyDownOnGraph);
        }

        void CreateVertWin()    //Bar Window generiern
        {
            _vbw = new VertBarWin();
            string[] names = new string[10];// { "1", "2", "3", "4", "5", "6" };    //Namen der Bars definieren
            for( var track = 0; track < 10; track++)
            {
                names[track] = ConfigurationManager.AppSettings.Get("S" + track + "Name");
            }
            _vbw.CreateBars2(names);
            _vbw.SetY1Scale(false,
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y1min")),
                Convert.ToInt32(ConfigurationManager.AppSettings.Get("Y1max"))); //Scale
            _vbw.AxisChange();
        }

        void SetupSliders() //Slider Generieren
        {
            _sb.ms[0].SetRange(-100, 100, 1); _sb.ms[0].cb = this;      //Slider 1 Range definieren
            _sb.ms[0].Text = "1";    //Slider 1 Name definieren
            _sb.ms[1].SetRange(-1000, 1000, 1); _sb.ms[1].cb = this;    //Slider 2 Range definieren
            _sb.ms[1].Text = "2";     //Slider 2 Name definiern
            _sb.ms[2].SetRange(-100, 100, 1); _sb.ms[2].cb = this;    //Slider 3 Range definieren
            _sb.ms[2].Text = "3";      //Slider 3 Name definieren
            _sb.ms[3].SetRange(-100, 100, 1); _sb.ms[3].cb = this;    //Slider 4 Rangee definieren
            _sb.ms[3].Text = "4";      //Slider 4 Name definieren

            /* _sb.ms[2].SetRange(0.1, 20.0, 0.1); _sb.ms[2].cb = this;
            _sb.ms[2].Text = "Mod Frequ.";
            _sb.ms[3].SetRange(0.1, 20.0, 0.1); _sb.ms[3].cb = this;
            _sb.ms[3].Text = "ModDev"; */
        }

        //TODO: Slider Wert senden
        public void OnValChanged(int aId, MSlider aSlider) // SliderCB  //Slider Wert senden
        {
            /*if (aId == 0)
            {
                // ph.binWr.WriteSv16((byte)2, (short)aSlider.val);
                _ph.binWr.Write((byte)6);   //ID 6
                _ph.binWr.Write((float)aSlider.val); //Slider wert als float auf COM schreiben
            }*/

            /* aId += 2;
            if (aId >= 2 && aId <= 5)
              ph.binWr.WriteSv16((byte)aId, (short)aSlider.val); */
            /* if (aId == 0) {
              ph.binWr.Write((byte)2);
              ph.binWr.Write((float)aSlider.val);
            } */

            //_ph.binWr.Flush();
        }

        public void OnMoseUp(int aId, MSlider aSlider) // SliderCB
        {
        }
    }
}
