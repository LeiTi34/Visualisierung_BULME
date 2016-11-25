using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Diagnostics;
using System.Globalization;
using ZedHL;
using System.IO;
using System.Windows.Forms;

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
            for (int i = 0; i < ivs.Length; i++)
                ivs[i] = new DummyValueSink();
            stw.Reset(); stw.Start(); //Stopuhr neustarten
        }

        public bool CheckValsPerSecond()    //Z‰hlt werte pro Sekunde
        {
            if (stw.ElapsedMilliseconds > 1000) //nach einer Sekunde
            {
                stw.Stop();     //Stopuhr anhalten
                valsPerSec = m_valSum / (stw.ElapsedMilliseconds / 1000.0); //Berechnen der Werte pro Sekunde
                m_valSum = 0; stw.Reset(); stw.Start(); //Wertez‰hler und Stopuhr zur¸cksetzen
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

        private void AddQueue()
        {

        }

        private void AddValue()
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

        public virtual void Close() //Stream schlieﬂen
        {
            binWr.Close();  //Binary Writer schlieﬂen
            m_BinRd.Close();    //Binary Reader schlieﬂen
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
     *  0...8   3.13 Format
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
                        vf[i - 10] = C1 * m_BinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die urspr¸ngliche ID wiederherzustellen)
                    else
                        vf[i - 10] = m_BinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die urspr¸ngliche ID wiederherzustellen)
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

        private const float C1 = (float)1.0 / short.MaxValue;

        private readonly Queue<float> logfloat = new Queue<float>();
        private readonly Queue<short> logshort = new Queue<short>();
        private readonly Queue<int> logchannel = new Queue<int>();

        private const int QueuMaxSize = 20 * 100;

        private int ID;
        private float value;

        private void AddQueue()
        {
            logfloat.Enqueue(value); //Daten in Query speichern
            logchannel.Enqueue(ID); //Kanalnummer in Query Speichern   

            if (logchannel.Count > QueuMaxSize)
            {
                logfloat.Dequeue();
                logchannel.Dequeue();
            }
        }

        private void AddValue()
        {
            vf[ID] = value;
            ivs[ID].AddValue(vf[ID]);
            AddQueue();
        }
        
        public override bool ParseAllPackets()
        {
            if (m_P.BytesToRead < 4) //Mindestens 3 Byte (ID + 2 Byte Daten)
            {
                return false;
            }

            while (m_P.BytesToRead >= 4)
            {
                var i = m_BinRd.ReadByte() - 1; //Liest erstes Byte (aID) -> wird vewendet um Datentyp zuzuordnen

                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());
                }
                else if (i >= 0 && i <= 8) //ID 0 bis 8: 3.13 Format
                {
                    value = m_BinRd.Read3P13();
                    ID = i;
                }
                else if (i >= 10 &&
                    i <= 19) //ID 10 bis 19: short (2 Byte)
                {
                    var buf = m_BinRd.ReadInt16();
                    ID = i - 10;
                    if (_scal == Scaling.Q15)
                    {
                        value = C1*buf;
                    }
                    else
                    {
                        value = buf;
                    }
                }
                else if (i >= 20 && i <= 29) //ID 20 bis 29: float
                {
                    value = m_BinRd.ReadSingle();
                    ID = i - 20;
                }

                if (m_BinRd.ReadByte() == 0xF)
                {
                    AddValue();
                }
                else
                {
                    while (m_BinRd.ReadByte() != 0xF) ;
                }
            }
            return true;
        }

        public override void SaveToCsv()
        {
            /*** TESTDATA ***
            float[] val = { (float)1.33, (float)1.44, (float)0, (float)2.33 };
            short[] chan = { 1, 2, 1, 2 };

            for (var i = 0; i <= 3; i++)
            {
                logfloat.Enqueue(val[i]); //Daten in Query speichern
                logchannel.Enqueue(chan[i]);
            }
            /*** TESTDATA END ***/

            //StreamWriter w writes on filePath
            var saveFileDialog1 = new SaveFileDialog();

            //FILENAME: Logs_<DATE>_<TIME>.csv
            var localDate = DateTime.Now;
            var culture = new CultureInfo("de-DE");
            saveFileDialog1.FileName = "Log_" + localDate.ToString(culture).Replace(".", "_").Replace(" ", "_").Replace(":", "_") + ".csv";
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
                        var line = "n;S1;S2;S3;S4;S5;S6;S7;S8;S9;S10";    //Spaltenbeschriftung
                        w.WriteLine(line);
                        w.Flush();

                        bool[] set = new bool[10];
                        string[] value = new string[10];

                        for (var i = 0; i < 10; i++)
                        {
                            set[i] = false;
                            value[i] = "";
                        }

                        //While data in Query
                        var n = 1;
                        while (logchannel.Count + logfloat.Count + logshort.Count > 0)
                        {
                            var fullchannel = logchannel.Dequeue(); //Get Channel-Number
                            int channel;

                            if (fullchannel >= 10 && fullchannel <= 19)
                            {
                                channel = fullchannel - 10;
                            }
                            else if (fullchannel >= 20 && fullchannel <= 29)
                            {
                                channel = fullchannel - 20;
                            }
                            else
                            {
                                channel = fullchannel;
                            }

                            if (set[channel])   //Write Line if value is already in Buffer
                            {
                                line = $"{n};{value[0]};{value[1]};{value[2]};{value[3]};{value[4]};{value[5]};{value[6]}";
                                w.WriteLine(line);
                                w.Flush();
                                n++;

                                for (var i = 0; i < 10; i++)    //Reset Values
                                {
                                    set[i] = false;
                                    value[i] = "";
                                }
                            }

                            if (fullchannel >= 10 && fullchannel <= 19)
                            {
                                value[channel] = logshort.Dequeue().ToString();    //Read from Query
                                set[channel] = true;
                            }
                            else if (fullchannel >= 20 && fullchannel <= 29) //ID 20 bis 29: float
                            {
                                value[channel] = logfloat.Dequeue().ToString().Replace(".", ",");  //Auf Europ‰isches Format umwandeln . -> ,
                                set[channel] = true;
                            }
                        }
                        //Restliche Werte auch Schreiben
                        line = $"{n};{value[0]};{value[1]};{value[2]};{value[3]};{value[4]};{value[5]};{value[6]};{value[7]};{value[8]};{value[9]}";
                        w.WriteLine(line);
                        w.Flush();
                    }
                    csvStream.Close();
                }
            }

            //Query Leeren
            logfloat.Clear();
            logchannel.Clear();
        }
    }
}
