using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Assignment3_WPF
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Show(); 
        }
        
        /* -------------------------- */
        /* ----- Button events ------ */
        private void regBtn_click(object sender, RoutedEventArgs e)
        {
            new PersonForm().ShowDialog();
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /* ----- Button events ----- */
        /* ------------------------- */
    }
}
