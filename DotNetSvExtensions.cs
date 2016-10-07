/*!
 * @file DotNetSvExtensions.cs
 * @author Bulme
 * @brief Beinhaltet veränderte .NET Klasse
 */ 
using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace vis1
{
    /*!
     * Liest Daten von Stream ein
     */

    internal class BinaryReaderEx : BinaryReader //Liest von Datenstrom (COM)
    {
        readonly byte[] _mCString = new byte[50]; //!< Buffer zum einlesen eines Strings im Byte format

        public BinaryReaderEx(Stream input) : base(input)
        {
        }

        /*!
         * Liest String von Datenstrom ein. 
         * Liest bis 0x0 empfangen wird und liefert einen string zurueck
         * @return Eingelesenen String
         */
        public string ReadCString() //String einlesen bis 0x0
        {
            var len = 0; //Enthaelt Sting-laenge

            while (true)
            {
                var ch = ReadByte();    //Hilfsvarieble zum einlesen von char

                if (ch == 0)    //Lesen bis Zeichen 0x0 eingelesen wird
                {
                    break;
                }

                _mCString[len] = ch; //Zeichen in Byte-Array schreiben
                len++;
            }
            var ret = Encoding.ASCII.GetString(_mCString, 0, len); // Byte Array in String umwandeln
            return ret;   //String zurückliefern
        }

        // 1.11 Format
        /*!
         * Einlesen des Fixkommaformates 1.11 (2Byte). Liefert Wert zurueck.
         */
        public float Read1P11()
        {
            return (float)ReadInt16() / 2048;
        }

        // 3.13 Format
        /*!
        * Einlesen des Fixkommaformates 3.13 (2Byte). Liefert Wert zurueck.
        */
        public float Read3P13()
        {
            return (float)ReadInt16() / 8192;
        }

        //protected virtual void Dispose(bool disposing);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /*!
     * Enthaelt Methoden zum Schreiben auf den Stream
     */
     class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream input) : base(input) { }   //Schreibt in Datenstrom (COM)

        /*!
         * Schreibt 1 Byte ID und 2 Byte Daten auf den Datenstrom
         * @param aId ID (1 Byte)
         * @param aVal Daten (2 Byte)
         */
        public void WriteSv16(byte aId, short aVal)
        {
            Write(aId);  //Sende ID
            Write(aVal); //Sende 2Byte Daten
                              /* this.Write((byte)aVal); // LB
                              this.Write((byte)(aVal >> 8)); // HB */

        }
    }

   /* class TrackBarEx : TrackBar
    {
        private int _mLastVal = 0;
        public bool BarWasMoved = false;

        public TrackBarEx()
          : base()
        {
        }

        protected override void OnValueChanged(EventArgs e)
        {
            BarWasMoved = true;
            base.OnValueChanged(e);
        }

        public bool BarValueChanged()
        {
            return Value != _mLastVal;
        }

        public short GetValue()
        {
            _mLastVal = Value;
            return (short)_mLastVal;
        }
    }*/


    class CommandParser
    {
        readonly BinaryWriter _binWr;

        public CommandParser(BinaryWriter aWr)
        {
            _binWr = aWr;
        }

        public void ParseAndSend(string aCmd)
        {
            var first = true;
            var words = aCmd.Split(' ');
            foreach (var txt in words)
            {
                var obj = Str2Val(txt);
                if (obj == null)
                    continue;
                if (first)
                {
                    var sv = (short)obj;
                    _binWr.Write((byte)sv); first = false;
                }
                else if (obj is int)
                {
                    var v32 = (Int32)obj;
                    _binWr.Write(v32);
                }
                else if (obj is float)
                {
                    var fv = (float)obj;
                    _binWr.Write(fv);
                }
                else
                {
                    var sv = (short)obj;
                    _binWr.Write(sv);
                }
            }
            _binWr.Flush();
        }

        /*!
         * Wandelt einen String je nach Index in einen Wert um
         * Index:
         * - l: Int  
         * - f: Float  
         * - ,: Float  
         * - /: Float (Teilt den Wert vor durch Wert nach dem '/'  
         * - Andere: Short
         */
        object Str2Val(string aTxt)
        {
            var txt2 = aTxt.Trim();
            if (txt2.Length == 0)
                return null;

            var idx = txt2.IndexOf('l');    //Index l: Int
            if (idx != -1)
            {
                txt2 = txt2.Remove(idx, 1);
                var val = int.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf('f');    //Index f: Float
            if (idx != -1)
            {
                txt2 = txt2.Remove(idx, 1);
                var val = float.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf(','); //Index ,: Float
            if (idx != -1)
            {
                var val = float.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf('/'); /* Index /: Float
                                      * Teilt String in Wert vor und nach '/' auf und Dividiert diese*/
            if (idx != -1)
            {
                var parts = txt2.Split('/');
                var val = float.Parse(parts[0]) / float.Parse(parts[1]);
                return val;
            }
            //Ohne Index: Short
            short sval;
            sval = short.Parse(txt2);
            return sval;
        }
    }
}
