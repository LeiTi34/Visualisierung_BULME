
using System;
// using System.Text;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;
using ZedHL;

namespace vis1
{
    public enum Scaling
    {
        None = 0,
        q15 = 1,
    }

    class Scaling2
    {
        public const float q15 = (float)1.0 / (float)Int16.MaxValue;
        public const float q11 = (float)1.0 / (float)2048;
    }

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
                valsPerSec = (double)m_valSum / ((double)stw.ElapsedMilliseconds / 1000.0); //Berechnen der Werte pro Sekunde
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

        // parses all ProtocolPacket's with all Variables
        public virtual bool ParseAllPackets()
        {
            return false;
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


    class NxtProtocolHandler : ProtocolHandler
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
    }


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


    class SvIdProtocolHandler3 : SvIdProtocolHandler
    {
        const float C1 = (float)1.0 / (float)Int16.MaxValue;

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

                ///CHANGE: continue mit else if ersetzt!
                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());
                    //continue;
                }
                else if (i >= 0 && i <= 8)  //ID 0 bis 8: 3.13 Format
                {
                    vf[i] = m_BinRd.Read3p13();
                    ivs[i].AddValue(vf[i]);
                    //continue;
                }
                else if (i >= 10 && i <= 19)    //ID 10 bis 19: short (2 Byte)
                {
                    if (_scal == Scaling.q15) //_scal == 1
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


    class SvIdProtocolHandler2 : SvIdProtocolHandler
    {
        public SvIdProtocolHandler2(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
        }

        public override bool ParseAllPackets()
        {
            if (m_P.BytesToRead < 3)    //Mindestens 2 Byte (ID + Daten)
            {
                return false;
            }

            int i;  //ID

            while (m_P.BytesToRead >= 3)
            {
                i = m_BinRd.ReadByte() - 1; //Einlesen von ID

                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());    //String einlesen und ausgeben
                    //continue;
                }
                else if (i >= 0 && i <= 3)
                {
                    vf[i] = m_BinRd.Read1p11();
                }

                // if( i>=1 && i<=3 ) vf[i] = m_BinRd.ReadInt16();

                ivs[i].AddValue(vf[i]);
            }
            return true;
        }
    }


    class HPerfProtocolHandler : SvIdProtocolHandler
    {
        public HPerfProtocolHandler(SerialPort aPort, IPrintCB aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 4; NBytes = 3 * NVals;
        }

        public override bool ParseAllPackets()
        {
            if (m_P.BytesToRead < 3)    //Mindestens 2 Byte (ID + Daten)
            {
                return false;
            }

            int i;  //ID

            while (m_P.BytesToRead >= 3)
            {
                i = m_P.ReadByte() - 1; //ID einlesen

                if (i == 9) //ID 9: string SV
                {
                    _printCB.DoPrint(m_BinRd.ReadCString());    //String einlesen und ausgeben
                }
                else if (i >= 0 && i <= 1) //ID 1 bis 2: float-SV
                {
                    vf[i] = m_BinRd.ReadSingle();   //4 Byte float einlesen
                    // ivs[i].AddValue(vf[i]);
                }
                else if (i >= 2 && i <= 3)  //ID 2 bis 3: ??
                {
                    int NVals = (byte)m_P.ReadByte();
                    brb[i].AddBytes(m_P.BaseStream, 2 * NVals);
                }
            }
            return true;
        }
    }
}
