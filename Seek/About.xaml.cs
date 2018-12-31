using System;
using System.Windows;

namespace Seek
{
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();

            tbVersionAuthor.Text = "Version 1.0\n\nCopyright \u00A9 " + DateTime.Now.Year + "\nOkpara Obinna Innocent";
            tbUser.Text = String.Format("Used by {0} on {1}", Environment.UserName, Environment.MachineName);
            tbDisclaimer.Text = "Disclaimer \nThis project is offered totally for free and is intended for legal use. The software and " +
                "all members of the development team will not be held responsible for any issue that may arise from use of this software.";
        }
    }
}
