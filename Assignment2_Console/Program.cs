using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Assignment2_Console;

class Program
{
    class PriorityQueue<T>(Func<T, int> priorityFunction, bool ascending = true, int capacity = 1) 
        where T : new()
    {
        private readonly List<T> _list = new (capacity);
        
        private void BinaryInsertion(T value)
        {
            (int min, T _) = BinarySearch(value);
            if (_list.Count < 1)
            {
                _list.Add(value);
                return;
            }
            
            _list.Insert(min, value);
        }
        
        public (int, T) BinarySearch(T value)
        {
            int min = 0;
            int max = _list.Count - 1;
            
            // Sign of priority is flipped in case of descending order
            int priority = priorityFunction(value) * (ascending ? 1 : -1);
            while (min < max)
            {
                int mid = (min + max) / 2;
                int midPriority = priorityFunction(_list[mid]);

                if (midPriority < priority) max = mid;
                else min = mid + 1;
            }

            return min switch
            {
                _ when !_list.Any() => (min, new T()),
                _ => (min, value)
            };
        }
        
        public void Enqueue(T value) => BinaryInsertion(value);
        
        public T Dequeue()
        {
            T value = _list.Last();
            _list.RemoveAt(_list.Count - 1);
            return value;
        }
        
        public int Count => _list.Count;
    }
    
    readonly struct SalesPerson(string name, string id, string district, int sales)
    {
        public string Name { get; init; } = name;
        public string Id { get; init; } = id;
        public string District { get; init; } = district;
        public int Sales { get; init; } = sales;

        public SalesPerson() : this("", "", "", 0){}
    }

    class Column(int capacity = 1)
    {
        public List<string> Values { get; set; } = new (capacity);
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
        
    class ColumnWriter(int columnCount, int columnSpacing = 2)
    {
        public readonly int ColumnCount = columnCount;
        private readonly List<Column> _columns = new (columnCount);
        private readonly List<int> _columnWidths = new (columnCount);
        private int _columnSelector = 0;

        public void Add(string value)
        {
            while (_columns.Count < ColumnCount)
            {
                _columns.Add(new Column());
                _columnWidths.Add(0);
            }
            
            _columnSelector %= ColumnCount;
            
            _columns[_columnSelector].Add(value);
            _columnWidths[_columnSelector] = Math.Max(_columnWidths[_columnSelector], value.Length);
            _columnSelector++;
        }
        
        public void Write()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _columns[0].Values.Count; i++)
            {
                for (int j = 0; j < ColumnCount; j++)
                {
                    sb.Append($"{_columns[j][i]}");
                    sb.Append(" ".PadRight(_columnWidths[j] + columnSpacing));
                }
                
                sb.AppendLine();
            }
            Console.Write(sb.ToString());
        }
    }
    
    static void PrintSalesData(string path)
    {
        int[] salesTiers = { 50, 100, 200 };
        // Read all lines from 'sales_data' file
        string[] lines = File.ReadAllLines(path);
        string[] headers = lines[0].Split(',');
        ColumnWriter columnWriter = new ColumnWriter(headers.Length);
        foreach (string header in headers) columnWriter.Add(header);
        PriorityQueue<SalesPerson> salesPeoplePq = new Program.PriorityQueue<SalesPerson>(salesPerson => salesPerson.Sales, true, lines.Length - 1);
        
        // Loop through all lines in 'sales_data' file
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            salesPeoplePq.Enqueue(new SalesPerson(values[0], values[1], values[2], int.Parse(values[3])));
        }
        
        if (salesPeoplePq.Count == 0) return;
        
        // Use binary search to find the boundary indices of the sales tiers
        int[] salesTiersIndices = new int[salesTiers.Length];
        for (int i = 0; i < salesTiers.Length; i++)
        {
            int salesTier = salesTiers[i];
            int salesTierIndex = salesPeoplePq.BinarySearch(new SalesPerson("", "", "", salesTier)).Item1;
            if (salesTierIndex >= salesPeoplePq.Count)
            {
                Console.Write("Index out of range error. Press enter to continue...");
                Console.ReadLine();
                salesTierIndex = salesPeoplePq.Count - 1;
            }
            
            salesTiersIndices[i] = (salesPeoplePq.Count - 1) - salesTierIndex;
        }
        
        int idx = 0;
        while (salesPeoplePq.Count > 0)
        {
            SalesPerson salesPerson = salesPeoplePq.Dequeue();
            columnWriter.Add($"|{salesPerson.Name}");
            columnWriter.Add($"|{salesPerson.Id}");
            columnWriter.Add($"|{salesPerson.District}");
            columnWriter.Add($"|{salesPerson.Sales.ToString()}");

            if (salesPerson.Name == "Test6")
            {
                Console.WriteLine("Test6");
            }
            
            // Use the boundary indices to determine if we should print a sales tier
            if (salesTiersIndices.Contains(idx) ||
                (salesPeoplePq.Count == 0 && salesPerson.Sales >= salesTiers.Last()))
            {
                // Determine the tier level
                int tierLevel = salesTiersIndices.ToList().IndexOf(idx) + 1;
                // If the tier level is 0, then we are in the highest tier due to how we calculate the indices
                if (tierLevel == 0) tierLevel = salesTiers.Length + 1;
                
                // Determine the range of the tier, we have custom logic for the lowest and highest tiers
                int low = (tierLevel > 1) ? salesTiers[tierLevel - 2] : 0;
                int high = (tierLevel != salesTiersIndices.Length + 1) ? salesTiers[tierLevel - 1] : salesPerson.Sales;
                bool highestTier = tierLevel > salesTiers.Length;
                
                // Create a range object to print the range of the tier
                Range range = new Range(low, high);
                // The highest tier has no upper bound and would therefore be inaccurate to present as a range
                if (!highestTier)
                {
                    columnWriter.Add($"Sales Tier: {tierLevel} ({range.Start} - {range.End})");
                }
                else
                {
                    // According to previous comments, Start+ is used to indicate lack of upper bound
                    columnWriter.Add($"Sales Tier: {tierLevel} ({range.Start}+)");
                }
                for (int i = 0; i < columnWriter.ColumnCount - 1; i++) columnWriter.Add("");
                
                // Add a separator
                columnWriter.Add("".PadLeft(20, '-'));
                for (int i = 0; i < columnWriter.ColumnCount - 1; i++) columnWriter.Add("");
            }

            idx++;
        }
        
        // The column writer has all the strings it can figure out all needed padding and spacing for each column
        columnWriter.Write();
    }
    
    // Simple method to write a message in a specific color
    static void WriteLineInColor(string message, ConsoleColor color)
    {
        ConsoleColor PrevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = PrevColor;
    }
    
    // This will be something like a header for the program
    static void ClearAndPrintPrefix()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        
        Console.Clear();
        Console.WriteLine("".PadLeft(20, '='));
        Console.WriteLine("SalesSystem");
        Console.WriteLine($"Culture: {culture.Name}");
        
        // Set underline (ansi escape sequence)
        WriteLineInColor("Säljarstatistik:", ConsoleColor.Cyan);
        WriteLineInColor("".PadLeft(20, '-'), ConsoleColor.Cyan);
        PrintSalesData("sales_data");
        WriteLineInColor("".PadLeft(20, '-'), ConsoleColor.Cyan);
        
        Console.WriteLine("".PadLeft(20, '='));
    }
    
    // Prompt the user for a sales person
    /*
     * Features:
     * - Regex validation
     * - Cursor repositioning
     */
    static SalesPerson PromptSalesPerson()
    {
        // Prompt the user for a sales person and setup regex patterns
        string userInput = "";
        ClearAndPrintPrefix();
        Console.WriteLine("Lägg till försäljning");
        // Capital letter followed by a word or words separated by a space
        // This will match trailing spaces as well
        string namePattern = @"^(([A-Z])\w* {0,1})+$";
        // 10-12 digits
        string idPattern = @"^\d{10,12}$";
        string salesPattern = @"^\d+$";
        
        // Loop until: namePattern is matched
        Console.WriteLine();
        while (!System.Text.RegularExpressions.Regex.IsMatch(userInput, namePattern))
        {
            (int _, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top - 1);
            Console.Write("Namn: ");
            userInput = Console.ReadLine() ?? "";
        }
        string name = userInput;
        userInput = "";
        
        // Loop until: idPattern is matched
        Console.WriteLine();
        while (!System.Text.RegularExpressions.Regex.IsMatch(userInput, idPattern))
        {
            (int _, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top - 1);
            Console.Write("Personnummer: ");
            userInput = Console.ReadLine() ?? "";
        }
        string id = userInput;
        userInput = "";
        
        // Loop until: district logic is fulfilled
        // Anything not fully numeric
        Console.WriteLine();
        while (System.Text.RegularExpressions.Regex.IsMatch(userInput, @"^\d+$") || userInput.Length < 1)
        {
            (int _, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top - 1);
            Console.Write("Distrikt: ");
            userInput = Console.ReadLine() ?? "";
        }
        string district = userInput;
        userInput = "";
        
        // Loop until: salesPattern is matched
        Console.WriteLine();
        while (!System.Text.RegularExpressions.Regex.IsMatch(userInput, salesPattern))
        {
            (int _, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top - 1);
            Console.Write("Försäljning: ");
            userInput = Console.ReadLine() ?? "";
        }
        int sales = int.Parse(userInput);
        Console.WriteLine();
        
        return new SalesPerson(name, id, district, sales);
    }

    static void WriteSalesPerson(SalesPerson salesPerson, string path)
    {
        // Append to 'sales_data' file
        using StreamWriter sw = File.AppendText(path);
        sw.WriteLine($"{salesPerson.Name},{salesPerson.Id},{salesPerson.District},{salesPerson.Sales}");
    }
    
    static void Main(string[] args)
    {
        // Create 'sales_data' extensionless file if it doesn't exist
        if (!System.IO.File.Exists("sales_data"))
        {
            FileStream fs = System.IO.File.Create("sales_data");
            // Add headers to 'sales_data' file
            StreamWriter sw = new StreamWriter(fs);
            sw.WriteLine("Namn,Personnummer,Distrikt,Försäljning");
            sw.Close();
            fs.Close();
        }
        
        // Loop until user enters
        bool mainLoop = true;
        while (mainLoop)
        {
            ClearAndPrintPrefix();
            Console.WriteLine("1. Lägg till försäljning\n" +
                              "2. Avsluta");
            Console.Write("Välj: ");
            string userInput = Console.ReadLine() ?? "";
            switch (userInput)
            {
                case "1":
                    SalesPerson salesPerson = PromptSalesPerson();
                    WriteSalesPerson(salesPerson, "sales_data");
                    break;
                case "2":
                    mainLoop = false;
                    continue;
            }
        }
        
        Console.WriteLine("Tryck på valfri tangent för att avsluta...");
        Console.ReadKey();
    }
}