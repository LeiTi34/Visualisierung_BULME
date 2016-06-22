using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace vis1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VisForm3()); //Begins running a standard application message loop on the current thread, and makes the specified form visible. https://msdn.microsoft.com/en-us/library/system.windows.forms.application.run(v=vs.110).aspx 2016/09/09        }
        }
    }
}