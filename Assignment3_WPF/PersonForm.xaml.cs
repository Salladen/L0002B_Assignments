using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Assignment3_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PersonForm : Window
    {
        private Dictionary<TextBox, Regex> _regexDict;
        private Dictionary<TextBox, String> _templateDict;
        private const string ResTbDefualt = "Förnamn:\nEfternamn:\nPersonNr:";

        public PersonForm()
        {
            Hide();
            InitializeComponent();
            _regexDict = new Dictionary<TextBox, Regex> {
                {givenNameTB, Person.GivenNamePat},
                {surNameTB, Person.SurNamePat},
                {idNumTB, Person.IdNumPat}
            };
            _templateDict = new Dictionary<TextBox, String> {
                {givenNameTB, "Förnamn:"},
                {surNameTB, "Efternamn:"},
                {idNumTB, "ÅÅMMDD-XXXX:"}
            };
            givenNameTB.Text = _templateDict[givenNameTB];
            surNameTB.Text = _templateDict[surNameTB];
            idNumTB.Text = _templateDict[idNumTB];
        }
           
        /* -------------------------- */
        /* ----- Button events ------ */
        private void regBtn_click(object sender, RoutedEventArgs e)
        {
            resTB.Text = String.Join('\n', $"Förnamn: {givenNameTB.Text}",
                $"Efternamn: {surNameTB.Text}", $"Personnummer: {idNumTB.Text}");
            
            Person person = new Person(givenNameTB.Text, surNameTB.Text,
                idNumTB.Text);
            (bool, Person.Gender?) validationResult = person.Validate();
            if (!validationResult.Item1)
            {
                genderTB.Background = Brushes.PaleVioletRed;
                genderTB.Text = "Felaktig inmatning! Försök igen.";
                resTB.Text = ResTbDefualt;
                return;
            }
            
            genderTB.Background = Brushes.PaleGreen;
            genderTB.Text = validationResult.Item2 switch {
                Person.Gender.Male => "Man",
                Person.Gender.Female => "Kvinna",
                _ => "Inget kön"
            };
        }

        private void exitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /* ----- Button events ----- */
        /* ------------------------- */
        
        /* ----- TextField events ----- */
        /* ---------------------------- */
        private void TB_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender ?? throw new NullReferenceException();
            if (tb.Text != _templateDict[tb])
            {
                return;
            }
            
            tb.Text = "";
        }
        
        private void TB_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender ?? throw new NullReferenceException();
            if (_regexDict[tb].IsMatch(tb.Text))
            {
                return;
            }
            
            tb.Text = _templateDict[tb];
        }
        
        private void givenNameTB_GotFocus(object sender, RoutedEventArgs e)
        {
            TB_GotFocus(sender, e);
        }

        private void givenNameTB_LostFocus(object sender, RoutedEventArgs e)
        {
            TB_LostFocus(sender, e);
        }

        private void surNameTB_GotFocus(object sender, RoutedEventArgs e)
        {
            TB_GotFocus(sender, e);
        }

        private void surNameTB_LostFocus(object sender, RoutedEventArgs e)
        {
            TB_LostFocus(sender, e);
        }

        private void idNumTB_GotFocus(object sender, RoutedEventArgs e)
        {
            TB_GotFocus(sender, e);
        }
        
        private void idNumTB_LostFocus(object sender, RoutedEventArgs e)
        {
            TB_LostFocus(sender, e);
        }
        /* ----- TextField events ----- */
        /* ---------------------------- */
        
        /* ---------------------- */
        /* ----- Key events ----- */
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // If 'enter' is pressed, then treat it as if the register button was clicked
            if (e.Key == Key.Enter)
            {
                regBtn_click(sender, new RoutedEventArgs());
            }
        }
        /* ----- Key events ----- */
        /* ---------------------- */
    }
}