using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Assignment2_Console;

static class Program
{

    public static class RegexConstants
    {
        /* ** Shared strings ** */
        // Patterns of: (Capital letter followed by lowercase letters) concatenated by a space or hyphen
        private const string namePatBody = @"[A-ZÄÅÖ](?:[a-zäöå]*(?:[ -][A-ZÄÅÖ])*)*";
        // 10 or 12 digits
        private const string idPatBody = @"\d{10}|d{12}";
        // 1 or more digits
        private const string salesPatBody = @"\d+";
        /* ** Shared strings ** */
        
        // Data field patterns
        public static Regex namePattern = new Regex($"^{namePatBody}$", RegexOptions.CultureInvariant);
        public static Regex idPattern = new Regex($"^{idPatBody}$", RegexOptions.CultureInvariant);
        public static Regex salesPattern = new Regex($"^{salesPatBody}$", RegexOptions.CultureInvariant);
        
        // For parsing the output log file (please don't make me parse files like this)
        public static Regex logPattern = new Regex($@"({namePatBody})\ +({idPatBody})\ +({namePatBody})\ +({salesPatBody})\ +", RegexOptions.CultureInvariant);
        public static Regex logHeaderPattern = new Regex(@"(\w+)\ +(\w+)\ +(\w+)\ +(\w+)\ +", RegexOptions.CultureInvariant);
    }
    
    
    class PriorityQueue<T>(Func<T, int> priorityFunction, bool ascending = true, int capacity = 1) : IEnumerable<T>
    {
        private readonly List<T> _list = new (capacity);
        
        private void BinaryInsertion(T value)
        {
            // Find the index to insert the value at
            (int min, T _) = BinarySearch(value);
                
            // Insert the value at the index but only if the list is not empty
            if (_list.Any())
            {
                _list.Insert(min, value);
                return;
            }
            
            // If the list is empty, just add the value
            _list.Add(value);
        }
        
        private (int, T) BinarySearch(T value)
        {
            // Assume a search space of the entire list
            int min = 0;
            int max = _list.Count;
            
            
            // Modified priority function to account for ascending/descending order
            Func<T, int> modPriorityFunction = x => ascending ? priorityFunction(x) : -priorityFunction(x);
            
            // Sign of priority is flipped in case of descending order
            int priority = modPriorityFunction(value);
            
            // Halve the search space until we find the index to insert the value at
            while (min < max)
            {
                int mid = (min + max) / 2;
                int midPriority = modPriorityFunction(_list[mid]);

                if (midPriority < priority) max = mid;
                else min = mid + 1;
            }
            
            return (min, value);
        }
        
        public void Enqueue(T value) => BinaryInsertion(value);
        
        public T Dequeue()
        {
            T value = _list.Last();
            _list.RemoveAt(_list.Count - 1);
            return value;
        }
        
        public int Count => _list.Count;
        
        public bool Contains(T value)
        {
            (int index, T _) = BinarySearch(value);
            return index < _list.Count && (_list[index]?.Equals(value) ?? false);
        }
        
        public int Where(T value)
        {
            (int index, _) = BinarySearch(value);
            return index;
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Repeated dequeueing will suffice for 'enumerating' a priority queue
            while (_list.Any())
            {
                yield return Dequeue();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal readonly struct SalesPerson(string name, string id, string district, int sales)
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
            // Fill the the columns with empty columns if they are not filled
            while (_columns.Count < ColumnCount)
            {
                _columns.Add(new Column());
                _columnWidths.Add(0);
            }
            
            // Ensure cyclic column selection
            _columnSelector %= ColumnCount;
            
            // Add the value to the column and update the column width
            _columns[_columnSelector].Add(value);
            _columnWidths[_columnSelector] = Math.Max(_columnWidths[_columnSelector], value.Length);
            _columnSelector++;
        }
        
        public void Write()
        {
            // Opt for a string builder to minimize string operations
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _columns[0].Values.Count; i++)
            {
                // Append all columns for the current row and add padding
                for (int j = 0; j < ColumnCount; j++)
                {
                    sb.Append($"{_columns[j][i]}");
                    sb.Append(" ".PadRight(_columnWidths[j] + columnSpacing));
                }
                
                sb.AppendLine();
            }
            // Concatenate the string builder to a string and write it to the console
            Console.Write(sb.ToString());
        }
    }
    
    struct Tier
    {
        public Range Range { get; init; }
        public List<SalesPerson> SalesPeople { get; init; }

        public override bool Equals(object? obj)
        {
            if (obj is not Tier tier) return false;
            return Range.Equals(tier.Range);
        }
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Range);
        }
    }
    
    static void PrintSalesData(string path)
    {
        int[] salesTiers = { 50, 100, 200};
        // Read all lines from 'sales_data' file
        FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
        StreamReader sr = new StreamReader(fs);
        string[] lines = sr.ReadToEnd().Split('\n');
        sr.Close();
        fs.Close();
        
        // CSV logic
        // string[] headers = lines[0].Split(',');
        
        // Needlessly complex regex logic
        // Match the header line
        Match headerMatch = RegexConstants.logHeaderPattern.Match(lines[0]);
        if (!headerMatch.Success) return;
        string[] headers = new string[headerMatch.Groups.Count - 1];
        for (int i = 1; i < headerMatch.Groups.Count; i++)
        {
            headers[i-1] = headerMatch.Groups[i].Value;
        }
        
        ColumnWriter columnWriter = new ColumnWriter(headers.Length);
        foreach (string header in headers) columnWriter.Add(header);
        PriorityQueue<SalesPerson> salesPeoplePq = new Program.PriorityQueue<SalesPerson>(salesPerson => salesPerson.Sales, true, lines.Length - 1);
        
        /*
        // Loop through all lines in 'sales_data' file and parse it | CSV format
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            salesPeoplePq.Enqueue(new SalesPerson(values[0], values[1], values[2], int.Parse(values[3])));
        }
        */
        
        // Loop through all lines in 'sales_data' file and parse it | stdout log regex matching format????
        // ... who thought this was a good idea?
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            Match match = RegexConstants.logPattern.Match(line);
            if (!match.Success) continue;
            string name = match.Groups[1].Value;
            string id = match.Groups[2].Value;
            string district = match.Groups[3].Value;
            int sales = int.Parse(match.Groups[4].Value);
            salesPeoplePq.Enqueue(new SalesPerson(name, id, district, sales));
        }
        
        if (salesPeoplePq.Count == 0) return;
        
        SalesPerson[] salesPeople = new SalesPerson[salesPeoplePq.Count];
        int idx = 0;
        foreach (SalesPerson salesPerson in salesPeoplePq)
        {
            salesPeople[idx++] = salesPerson;
        }
        
        Dictionary<SalesPerson, Tier> salesPersonTiers = new ();
        List<Tier> tiers = new ();
        for (int t = 1; t <= salesTiers.Length + 1; t++)
        {
            int low = (t > 1) ? salesTiers[t - 2] : 0;
            int high = (t != salesTiers.Length + 1) ? salesTiers[t - 1] : int.MaxValue;
            
            // Create a range object to print the range of the tier
            Range range = new Range(low, high);
            tiers.Add(new Tier
            {
                Range = range,
                SalesPeople = new List<SalesPerson>()
            });
        }
        foreach (SalesPerson salesPerson in salesPeople)
        {
            foreach (Tier tier in tiers)
            {
                if (tier.Range.Start.Value <= salesPerson.Sales && salesPerson.Sales < tier.Range.End.Value)
                {
                    tier.SalesPeople.Add(salesPerson);
                    salesPersonTiers[salesPerson] = tier;
                    break;
                }
            }
        }
        
        for (int h = 0; h < salesPeople.Length; h++)
        {
            SalesPerson salesPerson = salesPeople[h];
            
            columnWriter.Add($"{salesPerson.Name}");
            columnWriter.Add($"{salesPerson.Id}");
            columnWriter.Add($"{salesPerson.District}");
            columnWriter.Add($"{salesPerson.Sales.ToString()}");
            
            Tier? prevTier = (h + 1 < salesPeople.Length) ? salesPersonTiers[salesPeople[h + 1]] : salesPersonTiers[salesPeople[h]];
            if (prevTier == null)
            {
                continue;
            }

            if (!salesPersonTiers[salesPerson].Equals(prevTier)  || h == salesPeople.Length - 1)
            {
                Tier tier = salesPersonTiers[salesPerson];
                int tierLevel = tiers.IndexOf(salesPersonTiers[salesPerson]) + 1;
                bool highestTier = tierLevel > salesTiers.Length;
                // The highest tier has no upper bound and would therefore be inaccurate to present as a range
                if (!highestTier)
                {
                    columnWriter.Add($"{tier.SalesPeople.Count} säljare har nått nivå {tierLevel}: {tier.Range.Start}-{tier.Range.End} artiklar");
                }
                else
                {
                    // According to previous comments, Start+ is used to indicate lack of upper bound
                    columnWriter.Add($"{tier.SalesPeople.Count} säljare har nått {tierLevel}: över {tier.Range.Start.Value-1} artiklar");
                }
                for (int i = 0; i < columnWriter.ColumnCount - 1; i++) columnWriter.Add("");
                
                // Add a separator
                columnWriter.Add("".PadLeft(0, '_'));
                for (int i = 0; i < columnWriter.ColumnCount - 1; i++) columnWriter.Add("");
            }
        }
        
        // The column writer has all the strings it can figure out all needed padding and spacing for each column
        columnWriter.Write();
    }
    
    static void DumpSalesDataLog(string path)
    {
        // Redirect the console output to a file and print the sales data to 'sales_data.log'
        TextWriter consoleOut = Console.Out;
        
        FileStream fs = File.Open("sales_data2.temp", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(fs);
        StreamReader sr = new StreamReader(fs);
        
        // Empty the file
        fs.SetLength(0);
        Console.SetOut(sw);
        PrintSalesData(path);
        fs.Seek(0, SeekOrigin.Begin);
        consoleOut.Write(sr.ReadToEnd());
        sw.Close();
        fs.Close();
        
        // Overwrite 'sales_data2' with 'sales_data2.temp'
        File.Delete(path);
        File.Move("sales_data2.temp", path);
        Console.SetOut(consoleOut);
    }
    
    // Simple method to write a message in a specific color
    static void WriteLineInColor(string message, ConsoleColor color)
    {
        ConsoleColor prevColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = prevColor;
    }
    
    // This will be something like a header for the program
    static void ClearAndPrintPrefix()
    {
        CultureInfo culture = CultureInfo.CurrentCulture;
        
        Console.Clear();
        Console.WriteLine("".PadLeft(20, '='));
        Console.WriteLine("SalesSystem");
        Console.WriteLine($"Culture: {culture.Name}");
        Console.WriteLine("".PadLeft(20, '='));
        Console.Write("\n\n");
        
        // PrintSalesData("sales_data2");
        DumpSalesDataLog("sales_data2");
        
        Console.Write("\n\n");
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
        
        // Letting regex patterns be interpreted instead of compiled
        // we're not using them extensively enough to warrant compilation
        
        // Capital letter followed by a word or words separated by a space
        Regex namePattern = RegexConstants.namePattern;
        // 10 or 12 digits
        Regex idPattern = RegexConstants.idPattern;
        Regex salesPattern = RegexConstants.salesPattern;
        
        // Loop until: namePattern is matched
        Console.WriteLine();
        while (!namePattern.IsMatch(userInput))
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
        while (!idPattern.IsMatch(userInput))
        {
            (int _, int top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, top - 1);
            Console.Write("Personnummer: ");
            userInput = Console.ReadLine() ?? "";
        }
        string id = userInput;
        userInput = "";
        
        // Loop until: district logic is fulfilled
        // Name of district can be any word or words separated by a space or a hyphen
        Console.WriteLine();
        while (!namePattern.IsMatch(userInput))
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
        while (!salesPattern.IsMatch(userInput))
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

    static void WriteSalesPersonCSV(SalesPerson salesPerson, string path)
    {
        // Append to 'sales_data' file
        using StreamWriter sw = File.AppendText(path);
        sw.WriteLine($"{salesPerson.Name},{salesPerson.Id},{salesPerson.District},{salesPerson.Sales}");
    }
    
    static void WriteSalesPersonLog(SalesPerson salesPerson, string path)
    {
        // Append to 'sales_data' file
        using StreamWriter sw = File.AppendText(path);
        // Temporarily clutter the log file that the program reads, then prints the real output to the log file
        sw.WriteLine($"{salesPerson.Name} {salesPerson.Id} {salesPerson.District} {salesPerson.Sales} ");
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
                    WriteSalesPersonLog(salesPerson, "sales_data2");
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