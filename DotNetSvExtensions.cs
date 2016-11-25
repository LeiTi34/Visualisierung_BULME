using System;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace vis1
{
    internal class BinaryReaderEx : BinaryReader //Liest von Datenstrom (COM)
    {
        private byte[] m_CString = new byte[50];

        public BinaryReaderEx(Stream input)
          : base(input)
        {
        }

        public string ReadCString() //String einlesen bis 0x0 oder 0xF
        {
            int len = 0;    
            byte ch;    //Hilfsvariable zum einlesen

            do
            {
                ch = ReadByte(); //Lesen von 1 Byte
                m_CString[len] = ch; //Zeichen in Byte-Array schreiben
                len++;
            } while (ch != 0x0 && ch != 0xF );
            string ret = Encoding.ASCII.GetString(m_CString, 0, len); //Byte Array in String umwandeln
            return ret;   //String zurückliefern
        }

        // 1.11 Format
        public float Read1P11()
        {
            return (float)ReadInt16() / 2048;
        }

        // 3.13 Format
        public float Read3P13()
        {
            return (float)ReadInt16() / 8192;
        }
    }

    internal class BinaryWriterEx : BinaryWriter
    {
        public BinaryWriterEx(Stream input)     //Schreibt in Datenstrom (COM)
          : base(input)
        {
        }

        public void WriteSv16(byte aId, short aVal)
        {
            Write(aId);  //Sende ID
            Write(aVal); //Sende 2Byte Daten
                              /* this.Write((byte)aVal); // LB
                              this.Write((byte)(aVal >> 8)); // HB */

        }
    }

    internal class TrackBarEx : TrackBar
    {
        private int _mLastVal; // = 0;
        public bool BarWasMoved; // = false;

        /*public TrackBarEx() : base()
        {
        }*/

        protected override void OnValueChanged(EventArgs e)
        {
            BarWasMoved = true;
            base.OnValueChanged(e);
        }

        public bool BarValueChanged()
        {
            return (Value != _mLastVal);
        }

        public short GetValue()
        {
            _mLastVal = Value;
            return (short)_mLastVal;
        }
    }


    internal class CommandParser
    {
        private BinaryWriter _binWr;

        public CommandParser(BinaryWriter aWr)
        {
            _binWr = aWr;
        }

        public void ParseAndSend(string aCmd)
        {
            object obj; bool first = true;
            string[] words = aCmd.Split(' ');
            foreach (string txt in words)
            {
                obj = Str2Val(txt);
                if (obj == null)
                    continue;
                if (first)
                {
                    short sv = (short)obj;
                    _binWr.Write((byte)sv); first = false;
                }
                else if (obj is int)
                {
                    Int32 v32 = (Int32)obj;
                    _binWr.Write(v32);
                }
                else if (obj is float)
                {
                    float fv = (float)obj;
                    _binWr.Write(fv);
                }
                else
                {
                    short sv = (short)obj;
                    _binWr.Write(sv);
                }
            }
            _binWr.Flush();
        }

        private object Str2Val(string aTxt)
        {
            int idx; string txt2;

            txt2 = aTxt.Trim();
            if (txt2.Length == 0)
                return null;

            idx = txt2.IndexOf('l');
            if (idx != -1)
            {
                Int32 val;
                txt2 = txt2.Remove(idx, 1);
                val = Int32.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf('f');
            if (idx != -1)
            {
                float val;
                txt2 = txt2.Remove(idx, 1);
                val = float.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf(',');
            if (idx != -1)
            {
                float val;
                val = float.Parse(txt2);
                return val;
            }
            idx = txt2.IndexOf('/');
            if (idx != -1)
            {
                float val;
                string[] parts = txt2.Split('/');
                val = float.Parse(parts[0]) / float.Parse(parts[1]);
                return val;
            }
            short sval;
            sval = short.Parse(txt2);
            return sval;
        }
    }
}
