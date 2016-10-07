/*!
 * @file ProtocolHandler.cs
 * @author Bulme
 * @brief Beinhaltet verschieden versionen von ProtocolHandler zum abarbeiten des Datenstroms
 */

// using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using ZedHL;

namespace vis1
{
    public enum Scaling
    {
        None = 0,
        Q15 = 1,
    }

    internal class Scaling2
    {
        public const float Q15 = (float)1.0 / short.MaxValue;
        public const float Q11 = (float)1.0 / 2048;
    }

    public interface IPrintCb
    {
        void DoPrint(string aTxt);
    }

    // TODO: Implement IDisposable
    internal class ProtocolHandler
    {
        #region Member Variables
        protected SerialPort MPort;   //!< Gibt Port der COM-Schnittstelle an
        protected BinaryReaderEx MBinRd; //!< Binary Reader
        protected Stopwatch Stw = new Stopwatch();  //!< Stopuhr
        protected int MValSum; //!< Summe der Werte fuer eine Sekunde
        protected IPrintCb PrintCb;
        #endregion

        #region Properties
        public Scaling Scal = Scaling.None;    //!< Skalierung
        public BinaryWriterEx BinWr;    //!< Binary Writer
        public short[] Vs = new short[10];  //!< Hilfsvariable zum einlesen von short Werten.
        public float[] Vf = new float[10];  //!< Hilfsvariable zum einlesen von float Werten.
        public IValueSink[] Ivs = new IValueSink[10];   //!< Uebergibt Wert an ZedGraph
        public ByteRingBuffer[] Brb = new ByteRingBuffer[10]; //!< Definiert Ringbuffer
        public int NVals;
        public int NBytes;   //!< NBytes Gibt die Mindetsanzahl der zu lesenden Bytes ein
        public double ValsPerSec;   //!< Enthaelt Werte/Sekunde
        #endregion

        /*!
         * Weist Port zu, ínitialisiert BinaryReader, -Writer und setzt Stopuhr zurueck
         * @param aPort COM Port
         * @param aPrintCB
         */
        public ProtocolHandler(SerialPort aPort, IPrintCb aPrintObj)
        {
            MPort = aPort;
            MBinRd = new BinaryReaderEx(MPort.BaseStream); //Liest von aPort
            BinWr = new BinaryWriterEx(MPort.BaseStream);   //Schreibt auf aPort
            PrintCb = aPrintObj;
            for (var i = 0; i < Ivs.Length; i++)
                Ivs[i] = new DummyValueSink();
            Stw.Reset(); Stw.Start(); //Stopuhr neustarten
        }

        /*! 
         * Zaehlt werte pro Sekunde
         * Nach mindestens einer Sekunde Werden die gezaehlten Werte durch die vergangene Zeit dividiert und somit die Werte pro Sekunde berechnet.
         * Danach wird die Stoppuhr und der Wetezaehler zurueckgesetzt.
         * Liefert false zuruck wenn weniger als eine Sekunde vergangen ist.
         * param ValsPerSec Berechnete Werte/Sekunde
         * 
         * @param m_valSum Gezaehlte Werte
         * @param stw.ElapsedMilliseconds Vergangen Zeit seit die Stoppuhr gestartet wurde.
         */
        public bool CheckValsPerSecond()
        {
            if (Stw.ElapsedMilliseconds <= 1000) return false;
            Stw.Stop();     //Stopuhr anhalten
            ValsPerSec = MValSum / (Stw.ElapsedMilliseconds / 1000.0); //Berechnen der Werte pro Sekunde
            MValSum = 0; Stw.Reset(); Stw.Start(); //Wertezähler und Stopuhr zurücksetzen
            return true;
        }

        public void Flush()
        {
            BinWr.Flush();
        }

        /*!
         * Schreibt 1 Byte ID + 2 Byte short Daten auf den Stream
         * @param aId ID
         * @param aVal Daten
         */
        public void WriteSv16(byte aId, short aVal)
        {
            BinWr.WriteSv16(aId, aVal);   //Sende ID + 2Byte Daten
        }

        // parses all ProtocolPacket's with all Variables
        public virtual bool ParseAllPackets()
        {
            return false;
        }

        public virtual void SwitchAcq(bool aOnOff)
        {
        }

        /*!
         * Schliesst BinaryWriter und BinaryReader
         */
        public virtual void Close() //Stream schließen
        {
            BinWr.Close();  //Binary Writer schließen
            MBinRd.Close();    //Binary Reader schließen
        }
    }

    internal class NxtProtocolHandler : ProtocolHandler
    {
        /*!
         * @class NxtProtocolHandl  ererbt von Protocol Handler ab
         */
        public NxtProtocolHandler(SerialPort aPort, IPrintCb aPrintObj)
          : base(aPort, aPrintObj)
        {
            //! Definiert NVals und NBytes
            NVals = 2; NBytes = 4; //! \param NBytes Minimale Anzahl an zu lesenden Byte
                                  
        }

        /*!
         * Sendet die ID 10 und entweder 0 oder 1
         * @param aOnOff    true -> sende 1, false -> sende 0
         */
        public override void SwitchAcq(bool aOnOff)
        {
            if (aOnOff)
            {
                BinWr.WriteSv16(10, 1);
                // binWr.Write((byte)1);
            }
            else
            {
                BinWr.WriteSv16(10, 0);
                // binWr.Write((byte)0);
            }
        }

        public override bool ParseAllPackets()
        {
            //float flV; //never used

            /*! Liest von stream ein wenn mindestens 4 Byte (2 Byte ID + 2 Byte Daten) gelesen weren koennen.
             * Uebergibt ivs[1],[2] je 2 Byte. Liefert false zurueck wenn mindestens 4 Byte gelesen werden koennen.
             * @param m_P.BytesToRead   Giebt Anzahl an Bytes an die vom Stream gelesen werden koennen
             * @param NBytes    Mindestanzahl an zu lesenden Bytes
             * @return Liefert false zurueck wenn weniger als 4 Bytes gelesen werden koennen
             */

            if (MPort.BytesToRead < NBytes)
            {
                return false;
            }

            while (MPort.BytesToRead >= NBytes)   //Lese 4Byte wenn mindestens 4 Byte im Recive Buffer stehen
            {
                // flV = m_BinRd.ReadSingle();
                // vs[0] = (short)flV; ivs[0].AddValue(flV);

                Vs[0] = MBinRd.ReadInt16();    //Lese 2 Byte
                Ivs[0].AddValue(Vs[0]);

                Vs[1] = MBinRd.ReadInt16();    //Lese 2 Byte
                Ivs[1].AddValue(Vs[1]);

                // m_valSum += NVals;
            }
            return true;
        }
    }


    internal class SvIdProtocolHandler : ProtocolHandler
    {
        public SvIdProtocolHandler(SerialPort aPort, IPrintCb aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 4;
            NBytes = 3 * NVals;
        }

        /*!
         * 
         * @param aOnOff    true-> sendet 0x11, false sendet 0x10
         */
        public override void SwitchAcq(bool aOnOff)// ?
        {
            if (aOnOff)
            {
                BinWr.Write((byte)1);
                BinWr.Write((byte)1);
            }
            else
            {
                BinWr.Write((byte)1);
                BinWr.Write((byte)0);
            }
        }

        /*!
         * Liest mindestens 3 Byte ein und erkennt anhand der ID das entsprechende Format.
         * ID 0 bis 3:  float value
         * ID 9:        string value
         * @param NVals gibt die ID fuer float values an (0 bis NVals)
         * @return  Liefert false zurueck wenn weniger als 3 Byte gelesen werden koennen
         */
        public override bool ParseAllPackets()  //Daten einlesen
        {
            if (MPort.BytesToRead < 3)    //Mindestens 3Byte (ID + 2Byte Data)
            {
                return false;
            }

            while (MPort.BytesToRead >= 3)
            {
                var i = MBinRd.ReadByte() - 1;

                if (i >= 0 && i < NVals) // ID 0 bis 3: float-SV
                {
                    Vf[i] = MBinRd.ReadSingle(); //4Byte float einlesen
                    Ivs[i].AddValue(Vf[i]);
                }
                if (i == 9) //ID 9:  string SV
                {
                    PrintCb.DoPrint(MBinRd.ReadCString());    //String einlesen und ausgeben
                }
            }
            return true;
        }
    }


    internal class SvIdProtocolHandler3 : SvIdProtocolHandler
    {
        private const float C1 = (float)1.0 / short.MaxValue;

        public SvIdProtocolHandler3(SerialPort aPort, IPrintCb aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 9; NBytes = 3 * NVals;
        }

        public override bool ParseAllPackets()
        {
            if (MPort.BytesToRead < 3)    //Mindestens 3 Byte (ID + 2 Byte Daten)
            {
                return false;
            }

            while (MPort.BytesToRead >= 3)
            {
                var i = MBinRd.ReadByte() - 1;  //ID

                //CHANGE: continue mit else if ersetzt!
                if (i == 9) //ID 9: string SV
                {
                    PrintCb.DoPrint(MBinRd.ReadCString());
                    //continue;
                }
                else if (i >= 0 && i <= 8)  //ID 0 bis 8: 3.13 Format
                {
                    Vf[i] = MBinRd.Read3P13();
                    Ivs[i].AddValue(Vf[i]);
                    //continue;
                }
                else if (i >= 10 && i <= 19)    //ID 10 bis 19: short value (2 Byte)
                {
                    if (Scal == Scaling.Q15) //_scal == 1
                        Vf[i - 10] = C1 * MBinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die ursprüngliche ID wiederherzustellen)
                    else
                        Vf[i - 10] = MBinRd.ReadInt16();   //Liest 2 Byte ein (von i wird 10 abgezogen um die ursprüngliche ID wiederherzustellen)
                    Ivs[i - 10].AddValue(Vf[i - 10]);
                    //continue;
                }
                else if (i >= 20 && i <= 29)    //ID 20 bis 29: float value
                {
                    Vf[i - 20] = MBinRd.ReadSingle();
                    Ivs[i - 20].AddValue(Vf[i - 20]);
                    //continue;
                }
            }
            return true;
        }
    }


    internal class SvIdProtocolHandler2 : SvIdProtocolHandler
    {
        public SvIdProtocolHandler2(SerialPort aPort, IPrintCb aPrintObj)
          : base(aPort, aPrintObj)
        {
        }

        public override bool ParseAllPackets()
        {
            if (MPort.BytesToRead < 3)    //Mindestens 2 Byte (ID + Daten)
            {
                return false;
            }

            while (MPort.BytesToRead >= 3)
            {
                var i = MBinRd.ReadByte() - 1;  //ID

                if (i == 9) //ID 9: string SV
                {
                    PrintCb.DoPrint(MBinRd.ReadCString());    //String einlesen und ausgeben
                    //continue;
                }
                else if (i >= 0 && i <= 3)
                {
                    Vf[i] = MBinRd.Read1P11();
                }

                // if( i>=1 && i<=3 ) vf[i] = m_BinRd.ReadInt16();

                Ivs[i].AddValue(Vf[i]);
            }
            return true;
        }
    }


    internal class HPerfProtocolHandler : SvIdProtocolHandler
    {
        public HPerfProtocolHandler(SerialPort aPort, IPrintCb aPrintObj)
          : base(aPort, aPrintObj)
        {
            NVals = 4; NBytes = 3 * NVals;
        }

        public override bool ParseAllPackets()
        {
            if (MPort.BytesToRead < 3)    //Mindestens 2 Byte (ID + Daten)
            {
                return false;
            }

            while (MPort.BytesToRead >= 3)
            {
                var i = MPort.ReadByte() - 1;  //ID

                if (i == 9) //ID 9: string SV
                {
                    PrintCb.DoPrint(MBinRd.ReadCString());    //String einlesen und ausgeben
                }
                else if (i >= 0 && i <= 1) //ID 1 bis 2: float-SV
                {
                    Vf[i] = MBinRd.ReadSingle();   //4 Byte float einlesen
                    // ivs[i].AddValue(vf[i]);
                }
                else if (i >= 2 && i <= 3)  //ID 2 bis 3: ??
                {
                    int nVals = (byte)MPort.ReadByte();
                    Brb[i].AddBytes(MPort.BaseStream, 2 * nVals);
                }
            }
            return true;
        }
    }
}
