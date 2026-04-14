using System;
using System.Linq;
using System.Windows.Forms;

namespace AudioPlayer;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new Form1(args.FirstOrDefault()));
    }
}
