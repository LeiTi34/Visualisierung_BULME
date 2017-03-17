using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Diagnostics;
using System.Globalization;
using ZedHL;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;

namespace vis1
{
    public enum Scaling
    {
        None = 0,
        Q15 = 1,
    }

    /*class Scaling2
    {
        public const float q15 = (float)1.0 / (float)Int16.MaxValue;
        public const float q11 = (float)1.0 / (float)2048;
    }*/

    public interface IPrintCB
    {
        void DoPrint(string aTxt);
    }


    class ProtocolHandler
    {
        #region Member Variables
        protected SerialPort m_P;
        protected BinaryReaderEx m_BinRd;
        protected Stopwatch stw = new Stopwatch();
        protected int m_valSum;
        protected IPrintCB _printCB;
        #endregion

        #region Properties
        public Scaling _scal = Scaling.None;
        public BinaryWriterEx binWr;
        public short[] vs = new short[10];
        public float[] vf = new float[10];
        public IValueSink[] ivs = new IValueSink[10];
        public ByteRingBuffer[] brb = new ByteRingBuffer[10];
        public int NVals, NBytes;
        public double valsPerSec;
        #endregion

        public bool SingleShotEnabled = false;
        public float SingleShotTrigger;
        public int SingleShotChannel;
        public bool SingleShotRunning;
        public bool SingleShotShow;
        protected int SingleShotCount = 0;

        public ProtocolHandler(SerialPort aPort, IPrintCB aPrintObj)
        {
            m_P = aPort;
            m_BinRd = new BinaryReaderEx(m_P.BaseStream); //Liest von aPort
            binWr = new BinaryWriterEx(m_P.BaseStream);   //Schreibt auf aPort
            _printCB = aPrintObj;
            for (var i = 0; i < ivs.Length; i++)
                ivs[i] = new DummyValueSink();
            stw.Reset(); stw.Start(); //Stopuhr neustarten
        }

        public bool CheckValsPerSecond()    //Z�hlt werte pro Sekunde
        {
            if (stw.ElapsedMilliseconds > 1000) //nach einer Sekunde
            {
                stw.Stop();     //Stopuhr anhalten
                valsPerSec = m_valSum / (stw.ElapsedMilliseconds / 1000.0); //Berechnen der Werte pro Sekunde
                m_valSum = 0; stw.Reset(); stw.Start(); //Wertez�hler und Stopuhr zur�cksetzen
                return true;
            }
            return false;
        }

        public void Flush()
        {
            binWr.Flush();
        }

        public void WriteSv16(byte aId, short aVal)
        {
            binWr.WriteSv16(aId, aVal);   //Sende ID + 2Byte Daten
        }

        public virtual void AddData()
        {
        }

        // parses all ProtocolPacket's with all Variables
        public virtual bool ParseAllPackets()
        {
            return false;
        }

        public virtual void SaveToCsv()
        {
        }

        public virtual void SwitchAcq(bool aOnOff)
        {
        }

        public virtual void Close() //Stream schlie�en
        {
            binWr.Close();  //Binary Writer schlie�en
            m_BinRd.Close();    //Binary Reader schlie�en
        }
    }

    /*
     * Liest zwei Short-Werte ein. Spur 1 und Spur 2
     */
    /*class NxtProtocolHandler : ProtocolHandler
    {
        public NxtProtocolHandler(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 2; NBytes = 4;
        }

        public override void SwitchAcq(bool aOnOff)
        {
            if (aOnOff)
            {
                binWr.WriteSv16(10, 1);
                // binWr.Write((byte)1);
            }
            else
            {
                binWr.WriteSv16(10, 0);
                // binWr.Write((byte)0);
            }
        }


        public override bool ParseAllPackets()
        {
            //float flV; //never used

            if (m_P.BytesToRead < NBytes)   //Mindestens 3 Byte lesen (ID + 2 Byte Daten)
            {
                return false;
            }

            while (m_P.BytesToRead >= NBytes)   //Lese 4Byte wenn mindestens 4 Byte im Recive Buffer stehen
            {
                // flV = m_BinRd.ReadSingle();
                // vs[0] = (short)flV; ivs[0].AddValue(flV);

                vs[0] = m_BinRd.ReadInt16();    //Lese 2 Byte
                ivs[0].AddValue(vs[0]);

                vs[1] = m_BinRd.ReadInt16();    //Lese 2 Byte
                ivs[1].AddValue(vs[1]);

                // m_valSum += NVals;
            }
            return true;
        }
    }*/

    /*
     * Liest folgende Werte bei IDs aus
     *  ID      Wert
     *  0...3   Float
     *  9       String
     */
    class SvIdProtocolHandler : ProtocolHandler
    {
        public SvIdProtocolHandler(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 4;
            NBytes = 3 * NVals;
        }

        public override void SwitchAcq(bool aOnOff)// ?
        {
            if (aOnOff)
            {
                binWr.Write((byte)1);
                binWr.Write((byte)1);
            }
            else
            {
                binWr.Write((byte)1);
                binWr.Write((byte)0);
            }
        }

        public override bool ParseAllPackets()  //Daten einlesen
        {
            int i;

            if (m_P.BytesToRead < 3)    //Mindestens 3Byte (ID + 2Byte Data)
            {
                return false;
            }

            while (m_P.BytesToRead >= 3)
            {
                i = m_BinRd.ReadByte() - 1; //Einlesen der ID

                if (i >= 0 && i < NVals) // ID 0 bis 3: float-SV
                {
                    vf[i] = m_BinRd.ReadSingle(); //4Byte float einlesen
                    ivs[i].AddValue(vf[i]);
                }
                if (i == 9) //ID 9:  string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());    //String einlesen und ausgeben
                }
            }
            return true;
        }
    }

    /*
     * Liest folgende Werte bei IDs aus
     *  ID      Wert
     *  0....8  3.13 Format
     *  9       String
     *  10...19 Short
     *  20...29 Float
     */
    class SvIdProtocolHandler3 : SvIdProtocolHandler
    {
        const float C1 = (float)1.0 / Int16.MaxValue;

        public SvIdProtocolHandler3(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 9; NBytes = 3 * NVals;
        }

        public override bool ParseAllPackets()
        {
            if (m_P.BytesToRead < 3)    //Mindestens 3 Byte (ID + 2 Byte Daten)
            {
                return false;
            }

            int i;  //ID

            while (m_P.BytesToRead >= 3)
            {
                i = m_BinRd.ReadByte() - 1; //Liest erstes Byte (aID) -> wird vewendet um Datentyp zuzuordnen

                //CHANGE: continue mit else if ersetzt!
                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());
                    //continue;
                }
                else if (i >= 0 && i <= 8)  //ID 0 bis 8: 3.13 Format
                {
                    vf[i] = m_BinRd.Read3P13();
                    ivs[i].AddValue(vf[i]);
                    //continue;
                }
                else if (i >= 10 && i <= 19)    //ID 10 bis 19: short (2 Byte)
                {
                    if (_scal == Scaling.Q15) //_scal == 1
                        vf[i - 10] = C1 * m_BinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die urspr�ngliche ID wiederherzustellen)
                    else
                        vf[i - 10] = m_BinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die urspr�ngliche ID wiederherzustellen)
                    ivs[i - 10].AddValue(vf[i - 10]);
                    //continue;
                }
                else if (i >= 20 && i <= 29)    //ID 20 bis 29: float
                {
                    vf[i - 20] = m_BinRd.ReadSingle();
                    ivs[i - 20].AddValue(vf[i - 20]);
                    //continue;
                }
            }
            return true;
        }
    }

    class BufProtocolHandler : SvIdProtocolHandler3
    {
        public BufProtocolHandler(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 9; NBytes = 3 * NVals;
        }
		//Counter for read errors
	    public int Readerrorcnt = 0;

		//Scaling factor
        private const float C1 = (float)1.0 / short.MaxValue;

		//Queues for exporting to File
        private readonly Queue<float> _logfloat = new Queue<float>();
        private readonly Queue<int> _logchannel = new Queue<int>();

		//Maximum size for queues TODO: sollte x-Axe entsprechen
        private const int QueuMaxSize = 20 * 100;

		//Add adae to queues and chart
	    private void AddData(int ID, float data)
        {
			//chart
            vf[ID] = data;
            ivs[ID].AddValue(vf[ID]);

            _logfloat.Enqueue(data); //Daten in Query speichern
            _logchannel.Enqueue(ID); //Kanalnummer in Query Speichern

			//Delete one item if queue maximum size is reched
            if (_logchannel.Count > QueuMaxSize)
            {
                _logfloat.Dequeue();
                _logchannel.Dequeue();
            }
        }

        public override bool ParseAllPackets()
        {   
            try
            {
                if (m_P.BytesToRead < 3) //Mindestens 3 Byte (ID + 2 Byte Daten)
                {
                    return false;
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Disconnected!");

                return false;
            }

            /*while (m_P.BytesToRead >= 3)
            {
                var id = m_BinRd.ReadByte() - 1; //Liest erstes Byte (aID) -> wird vewendet um Datentyp zuzuordnen

	            int track, type;

	            if (id == 9)	//Check for String
	            {
		            type = -1;
		            track = 0;
	            }
	            else	//Calculate other IDs
	            {
		            type = id/10;
                    track = id - 10*type;
                    Debug.WriteLine("ID: "+ id + "\tType: " + type + "\tTrack: " + track);
                }

	            if (track >= 0 && id <= track)
	            {
					switch (type)
					{
						case -1:	//string, bis 0
							_printCB.DoPrint( m_BinRd.ReadCString() );
							break;

						case 0:     //3P13,  4 Byte
							AddData(track, m_BinRd.Read3P13());
							break;

						case 1:     //short, 2 Byte
							switch (_scal)
							{
								case Scaling.Q15:
									AddData(track, C1*m_BinRd.ReadInt16());
									break;
								case Scaling.None:
									AddData(track, m_BinRd.ReadInt16());
									break;
							}
							break;

						case 2:     //float, 4 Byte
							AddData(id, m_BinRd.ReadSingle());
							break;

						default:
							Readerrorcnt++;
							break;
					}
				}
	            else
	            {
		            Readerrorcnt++;
	            }
			}*/

            while (m_P.BytesToRead >= 3)
            {
                int i = m_BinRd.ReadByte() - 1; //Liest erstes Byte (aID) -> wird vewendet um Datentyp zuzuordnen

                //CHANGE: continue mit else if ersetzt!
                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());
                    //continue;
                }
                else if (i >= 0 && i <= 8)  //ID 0 bis 8: 3.13 Format
                {
                    AddData(i, m_BinRd.Read3P13());
                    //continue;
                }
                else if (i >= 10 && i <= 19)    //ID 10 bis 19: short (2 Byte)
                {
                    if (_scal == Scaling.Q15) //_scal == 1
                        AddData(i - 10, C1 * m_BinRd.ReadInt16());
                    else
                        AddData(i - 10, m_BinRd.ReadInt16());
                    //continue;
                }
                else if (i >= 20 && i <= 29)    //ID 20 bis 29: float
                {
                    AddData(i - 20, m_BinRd.ReadSingle());
                    //continue;
                }
            }
            return true;
		}

		public override void SaveToCsv()
        {
            //StreamWriter w writes on filePath
            var saveFileDialog1 = new SaveFileDialog();

            //FILENAME: Logs_<DATE>_<TIME>.csv
            var localDate = DateTime.Now;
            var culture = new CultureInfo("de-DE");
            var date = localDate.ToString(culture).Replace(".", "_").Replace(" ", "_").Replace(":", "_");
            saveFileDialog1.FileName = "Log_" + date + ".csv";

            saveFileDialog1.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream csvStream;
                if ((csvStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (var w = new StreamWriter(csvStream))
                    {
                        var line = "n;S1;S2;S3;S4;S5;S6;S7;S8;S9;S10;";    //Spaltenbeschriftung
                        w.WriteLine(line);
                        w.Flush();

                        bool[] set = { false, false, false, false, false, false, false, false, false, false };
                        string[] value = { "", "", "", "", "", "", "", "", "", "" };

                        //While data in Query
                        var n = 1;
                        while (_logchannel.Count > 0 && _logfloat.Count > 0)
                        {
                            var channel = _logchannel.Dequeue(); //Get Channel-Number

                            if (set[channel])   //Write Line if value is already in Buffer
                            {
                                line = $"{n};{value[0]};{value[1]};{value[2]};{value[3]};{value[4]};{value[5]};{value[6]};{value[7]};{value[8]};{value[9]};";
                                w.WriteLine(line);
                                w.Flush();
                                n++;

                                for (var i = 0; i < 10; i++)    //Reset Values
                                {
                                    set[i] = false;
                                    value[i] = "";
                                }
                            }
                                value[channel] = _logfloat.Dequeue().ToString().Replace(".", ",");  //Auf Europ�isches Format umwandeln . -> ,
                                set[channel] = true;
                        }
                        //Restliche Werte auch Schreiben
                        line = $"{n};{value[0]};{value[1]};{value[2]};{value[3]};{value[4]};{value[5]};{value[6]};{value[7]};{value[8]};{value[9]};";
                        w.WriteLine(line);
                        w.Flush();
                    }
                    csvStream.Close();
                }
            }
            //Query Leeren
            _logfloat.Clear();
            _logchannel.Clear();
        }
	}
}
