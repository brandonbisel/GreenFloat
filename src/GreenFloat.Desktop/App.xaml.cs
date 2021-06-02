using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GreenFloat.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args != null && e.Args.Count() > 0)
            {
                this.Properties["CmdLineArgs"] = e.Args[0];
            }
            else
            {
                this.Properties["CmdLineArgs"] = "NoArgs";
            }

            base.OnStartup(e);
        }

    }
}
