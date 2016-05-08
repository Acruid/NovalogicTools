using System;
using System.Windows.Forms;

namespace vsk
{
    internal static class Program
    {
        internal static FormParent MainForm { get; private set; }

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new FormParent();
            Application.Run(MainForm);
        }
    }
}