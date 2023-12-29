using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TextBox = System.Windows.Controls.TextBox;

namespace Assignment1_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>


struct Money
{
    public uint Value { get; set; }
    public string Denomination { get; set; }
}

public partial class MainWindow : Window
{
    private readonly string _priceFieldPlaceHolder;
    private readonly string _paymentFieldPlaceHolder;
    // Some events involving this can be invoked during initialization
    // so we need to initialize it first then populate it
    private (Money, TextBox)[] _changeFields = new (Money, TextBox)[0];
    int _calculations = 0;

    public MainWindow()
    {
        _priceFieldPlaceHolder = "Pris";
        _paymentFieldPlaceHolder = "Betalt";
        
        InitializeComponent();
        _changeFields = new (Money, TextBox)[]
        {
            new(new Money {Value = 500, Denomination = "500-lapp"}, (TextBox)FindName("_500")),
            new(new Money {Value = 200, Denomination = "200-lapp"}, (TextBox)FindName("_200")),
            new(new Money {Value = 100, Denomination = "100-lapp"}, (TextBox)FindName("_100")),
            new(new Money {Value = 50, Denomination = "50-lapp"}, (TextBox)FindName("_50")),
            new(new Money {Value = 20, Denomination = "20-lapp"}, (TextBox)FindName("_20")),
            new(new Money {Value = 10, Denomination = "10-krona"}, (TextBox)FindName("_10")),
            new(new Money {Value = 5, Denomination = "5-krona"}, (TextBox)FindName("_5")),
            new(new Money {Value = 1, Denomination = "1-krona"}, (TextBox)FindName("_1")),
        };
    }
    
    // Helper Functions
    bool IsNumeric(string str)
    {   
        HashSet<char> uchars = str.ToHashSet();
        HashSet<char> digits = new HashSet<char>("0123456789");
        
        if (uchars.Except(digits).Any())
        {
            return false;
        }
        
        return true;
    }
    
    bool ParseMaxUInt(string str, out UInt32 result, Action<String> callback = null)
    {
        bool success = UInt32.TryParse(str, out result);
        if (!success && IsNumeric(str) && str.Length > 0)
        {
            result = UInt32.MaxValue;
            if (callback != null)
            {
                callback(result.ToString());
            }
            return true;
        }
        
        return success;
    }
    
    void CalculateChange(UInt32 difference)
    {
        // Return if _changeFields is null
        if (_changeFields == null) return;        

        _calculations++;
        Console.WriteLine($"Calculations: {_calculations}");
        // Loop through the denominations and their associated values
        foreach ((Money money, TextBox textBox) in _changeFields)
        {
            // The integer quotient is our change for that denomination
            (UInt32 amount, UInt32 remainder) = UInt32.DivRem(difference, money.Value);
            // And the remainder is what we have left to calculate
            difference = remainder;
            
            if (amount == 0)
            {
                textBox.Text = "";
            }
            else
            {
                textBox.Text = amount.ToString();
            }
        }
    }
    
    // Button Events
    private void exitBtn_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    // priceField Events
    private void priceField_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!ParseMaxUInt(priceField.Text, out _))
        {
            priceField.Text = _priceFieldPlaceHolder;
        }
    }
    
    private void setPriceFieldText(string text)
    {
        priceField.Text = text;
    }

    private void priceField_TextChanged(object sender, TextChangedEventArgs e)
    {
       if (ParseMaxUInt(priceField.Text, out UInt32 price, setPriceFieldText) &&
           ParseMaxUInt(paymentField.Text, out UInt32 payment, setPaymentFieldText) &&
           payment > price)
        {
            CalculateChange(payment - price);
        }
        else
        {
            foreach ((Money money, TextBox textBox) in _changeFields)
            {
                textBox.Text = "";
            }
        }
    }

    private void priceField_GotFocus(object sender, RoutedEventArgs e)
    {
        if (priceField.Text == _priceFieldPlaceHolder) priceField.Text = "";
    }

    // paymentField Events
    private void paymentField_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!ParseMaxUInt(paymentField.Text, out _))
        {
            paymentField.Text = _paymentFieldPlaceHolder;
        }
    }
    
    private void setPaymentFieldText(string text)
    {
        paymentField.Text = text;
    }
    
    private void paymentField_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ParseMaxUInt(paymentField.Text, out UInt32 payment, setPaymentFieldText) && 
            ParseMaxUInt(priceField.Text, out UInt32 price, setPaymentFieldText) && 
            payment > price)
        {
            CalculateChange(payment - price);
        }
        else
        {
            foreach ((Money money, TextBox textBox) in _changeFields)
            {
                textBox.Text = "";
            }
        }
    }

    private void paymentField_GotFocus(object sender, RoutedEventArgs e)
    {
        if (paymentField.Text == _paymentFieldPlaceHolder) paymentField.Text = "";
    }
}