using System;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ZedHL;

namespace vis1
{
    partial class VisForm3
    {
        public int Xmin;
        public int Xmax;

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

            _mSerPort = new SerialPort(comport, 115200, Parity.None, 8, StopBits.One) {ReadBufferSize = 20*1024};

            try
            {
                _mSerPort.Open(); //Serielle verbindung öffnen
            }
            catch (IOException)
            {
                MessageBox.Show(@"COM-Port timed out!");
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                MessageBox.Show(@"Could not open COM-Port:\n\n" + e);
                Environment.Exit(1);
            }
            finally
            {
                statusStrip.Items.Add("COM-Port opened successful");
            }

            _ph = new NewProtocolHandler(_mSerPort, this);
        }

        void SetupLineChart()
        {
            //ChartArea hinzufügen
            lineChart.ChartAreas.Add(_chartArea);

            //Spuren Initialisiern
            for (var channel = 0; channel < 10; channel++)
            {
                _series[channel] = new Series("S" + channel + 1)
                {
                    ChartType = SeriesChartType.Line,
                    ChartArea = "ChartArea",
                    BorderWidth = 2,
                    Legend = "Legend1",
                    Name = ConfigurationManager.AppSettings.Get("S" + channel + 1 + "Name"),
                    //IsXValueIndexed = true
                };
                lineChart.Series.Add(_series[channel]);
            }
        }

        /*void CreateVertWin()    //Bar Window generiern
        {
            _vbw = new VertBarWin();
            string[] names = { "1", "2", "3", "4", "5", "6" };    //Namen der Bars definieren
            _vbw.CreateBars2(names);
            _vbw.SetY1Scale(false, -10, 800); //Scale
            _vbw.AxisChange();
        }*/

        void SetupSliders() //Slider Generieren
        {
            //TODO App.Conf File
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
