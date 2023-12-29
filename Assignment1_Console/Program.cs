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
        public int Value { get; set; }
        public string Denomination { get; set; }
    }
    
    // UInt32 here again as where working with them in the main function
    static void CalculateChange(UInt32 remainder, Money[] moneyArray)
    {
        Console.WriteLine("Växel:");

        int[] maxColumnWidth = new int[3];
        List<string>[] columns = new List<string>[3];
        // Add the column headers
        for (int i = 0; i < 3; i++)
        {
            columns[i] = new List<string>();
        }
        
        columns[0].Add("Antal");
        maxColumnWidth[0] = columns[0][0].Length;
        columns[1].Add("Valör");
        maxColumnWidth[1] = columns[1][0].Length;
        columns[2].Add("Återstående");
        maxColumnWidth[2] = columns[2][0].Length;
        
        // Loop through the denominations and their associated values
        foreach (Money money in moneyArray)
        {
            // The integer quotient is our change for that denomination
            int amount = Math.DivRem((int)remainder, money.Value, out int remainderTemp);
            // And the remainder is what we have left to calculate
            remainder = (UInt32)remainderTemp;
            
            if (amount == 0) continue;
            // Use .2g format specifier on the amount
            columns[0].Add($"{amount:N0}");
            maxColumnWidth[0] = Math.Max(maxColumnWidth[0], columns[0].Last().Length);
            columns[1].Add(money.Denomination);
            maxColumnWidth[1] = Math.Max(maxColumnWidth[1], columns[1].Last().Length);
            columns[2].Add($"{remainder:C0}");
            maxColumnWidth[2] = Math.Max(maxColumnWidth[2], columns[2].Last().Length);
        }
        
        // Print the columns
        for (int i = 0; i < columns[0].Count; i++)
        {
            Console.Write($"|{columns[0][i].PadRight(maxColumnWidth[0])}");
            Console.Write(" ");
            Console.Write($"|{columns[1][i].PadRight(maxColumnWidth[1])}");
            Console.Write(" ");
            Console.Write($"|{columns[2][i].PadRight(maxColumnWidth[2])}");
            Console.WriteLine();
        }
        Console.WriteLine();
    }
    
    static string? ReadLine(string message)
    {
        Console.Out.Write(message);
        return Console.ReadLine();
    }

    static bool promptUInt(string message, out UInt32 result)
    {
        string userInput = Program.ReadLine(message) ?? "";
        result = 0;
        
        switch (userInput)
        {
            // If castable to double
            case var _ when UInt32.TryParse(userInput, out result):
                return true;
            default:
                return false;
        }
    }

    static void clearAndPrintPrefix()
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
        
        clearAndPrintPrefix();
        
        // Tying values to denominations
        Money[] moneyArray = new Money[]
        {
            new Money { Value = 500, Denomination = "femhundralappar" },
            new Money { Value = 200, Denomination = "tvåhundralappar" },
            new Money { Value = 100, Denomination = "hundralappar" },
            new Money { Value = 50, Denomination = "femtiolappar" },
            new Money { Value = 20, Denomination = "tjugolappar" },
            new Money { Value = 10, Denomination = "tiokronor" },
            new Money { Value = 5, Denomination = "femkronor" },
            new Money { Value = 1, Denomination = "enkronor" },
        };
        
        // Since we're not dealing with "negative" money, UInt32 is used to reflect this
        string? userInput;
        UInt32 price;
        UInt32 paid;
        
        bool mainLoop = true;
        while (mainLoop)
        {
            clearAndPrintPrefix();
            // Prompt for price
            while (!promptUInt($"Ange pris: ", out price))
            {
                Console.WriteLine("Felaktig inmatning!");
                Thread.Sleep(waitingTime);
                clearAndPrintPrefix();
            }
            
            // Prompt for payment sum
            while (!promptUInt("Betalt: ", out paid))
            {
                Console.WriteLine("Felaktig inmatning!");
                Thread.Sleep(waitingTime);
                clearAndPrintPrefix();
            }
            
            // If price is greater than paid
            if (price > paid)
            {
                Console.WriteLine("För lite betalt!");
                Thread.Sleep(waitingTime);
                clearAndPrintPrefix();
                continue;
            }
            else if (paid == price)
            {
                Console.WriteLine("Växel:");
                Console.WriteLine("Ingen växel behövs!");
            }
            else
            {
                UInt32 remainder = paid - price;
                Program.CalculateChange(remainder, moneyArray);
            }
            
            // Continuation prompt
            Console.WriteLine("Fortsätt (j/n)?");
            userInput = Console.ReadKey().KeyChar.ToString();
        
            if (userInput == "n")
            {
                Console.Clear();
                mainLoop = false;
            }
        }
        
        Console.WriteLine("Tryck på valfri tangent för att avsluta...");
        Console.ReadKey();
    }
}
