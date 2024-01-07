using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Assignment1_Console;

class Program
{
    // Struct to hold the denominations and their associated values
    struct Money
    {
        public int Value { get; init; }
        public string Denomination { get; init; }
    }

    class Column(int capacity = 10)
    {
        public List<string> Values { get; set; } = new List<string>(capacity);
        private int MaxWidth { get; set; } = 0;

        public void Add(string value)
        {
            Values.Add(value);
            MaxWidth = Math.Max(MaxWidth, value.Length);
        }
        
        public string this[int index]
        {
            get => Values[index].PadRight(MaxWidth);
            set {
                Values[index] = value;
                MaxWidth = Math.Max(MaxWidth, value.Length);
            }
        }
    }
    
    // uint here again as where working with them in the main function
    static void CalculateChange(uint remainder, Money[] moneyArray)
    {
        Console.WriteLine("Växel:");

        Column[] columns = new Column[3] { new Column(), new Column(), new Column() };
        
        columns[0].Add("Antal");
        columns[1].Add("Valör");
        columns[2].Add("Återstående");
        
        // Loop through the denominations and their associated values
        foreach (Money money in moneyArray)
        {
            // The integer quotient is our change for that denomination
            int amount = Math.DivRem((int)remainder, money.Value, out int remainderTemp);
            // And the remainder is what we have left to calculate
            remainder = (uint)remainderTemp;
            
            if (amount == 0) continue;
            // Use .2g format specifier on the amount
            columns[0].Add($"{amount:N0}");
            columns[1].Add(money.Denomination);
            columns[2].Add($"{remainder:C0}");
        }
        
        // Print the columns
        for (int i = 0; i < columns[0].Values.Count; i++)
        {
            Console.Write($"|{columns[0][i]}");
            Console.Write(" ");
            Console.Write($"|{columns[1][i]}");
            Console.Write(" ");
            Console.Write($"|{columns[2][i]}");
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    
    static string? ReadLine(string message)
    {
        Console.Out.Write(message);
        return Console.ReadLine();
    }
    
    static bool IsNumeric(string str)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(str, @"^\d+$");
    }

    static bool ParseMaxUInt(string str, out UInt32 result, Action<String>? callback = null)
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

    static bool PromptUInt(string message, out uint result)
    {
        void OverFlowCallback(string value) => Console.WriteLine($"För stort värde, begränsat till {value}!");
        string userInput = ReadLine(message) ?? "";
        result = 0;
        
        return userInput switch
        {
            _ when ParseMaxUInt(userInput, out result, OverFlowCallback) => true,
            _ => false
        };
    }

    static void ClearAndPrintPrefix()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        
        Console.Clear();
        Console.WriteLine("".PadLeft(20, '='));
        Console.WriteLine("VäxelSystem");
        Console.WriteLine($"Culture: {culture.Name}");
        // Currency symbol gets "kr" for Swedish culture but we do this for SEK
        Console.WriteLine($"Currency: {(culture.Name == "sv-SE" ? "SEK" : culture.NumberFormat.CurrencySymbol)}");
        Console.WriteLine("".PadLeft(20, '='));
    }
    
    static void Main(string[] args)
    {
        // Set culture to Swedish
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");
        // Using a TimeSpan to avoid magic numbers
        TimeSpan waitingTime = TimeSpan.FromSeconds(3);
        
        ClearAndPrintPrefix();
        
        // Tying values to denominations
        Money[] moneyArray = {
            new Money { Value = 500, Denomination = "femhundralappar" },
            new Money { Value = 200, Denomination = "tvåhundralappar" },
            new Money { Value = 100, Denomination = "hundralappar" },
            new Money { Value = 50, Denomination = "femtiolappar" },
            new Money { Value = 20, Denomination = "tjugolappar" },
            new Money { Value = 10, Denomination = "tiokronor" },
            new Money { Value = 5, Denomination = "femkronor" },
            new Money { Value = 1, Denomination = "enkronor" }
        };
        
        // Since we're not dealing with "negative" money, uint is used to reflect this

        bool mainLoop = true;
        while (mainLoop)
        {
            ClearAndPrintPrefix();
            // Prompt for price
            uint price;
            while (!PromptUInt("Ange pris: ", out price))
            {
                Console.WriteLine("Felaktig inmatning!");
                Thread.Sleep(waitingTime);
                ClearAndPrintPrefix();
            }
            
            // Prompt for payment sum
            uint paid;
            while (!PromptUInt("Betalt: ", out paid))
            {
                Console.WriteLine("Felaktig inmatning!");
                Thread.Sleep(waitingTime);
                ClearAndPrintPrefix();
            }
            
            // If price is greater than paid
            if (price > paid)
            {
                Console.WriteLine("För lite betalt!");
                Thread.Sleep(waitingTime);
                ClearAndPrintPrefix();
                continue;
            }

            if (paid == price)
            {
                Console.WriteLine("Växel:");
                Console.WriteLine("Ingen växel behövs!");
            }
            else
            {
                uint remainder = paid - price;
                CalculateChange(remainder, moneyArray);
            }

            // Continuation prompt
            Console.WriteLine("Fortsätt (j/n)?");
            var userInput = Console.ReadKey().KeyChar.ToString();

            if (userInput != "n") continue;
            Console.Clear();
            mainLoop = false;
        }
        
        Console.WriteLine("Tryck på valfri tangent för att avsluta...");
        Console.ReadKey();
    }
}
