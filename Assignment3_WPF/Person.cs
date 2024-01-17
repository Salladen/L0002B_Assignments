using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;

namespace Assignment3_WPF;

public class Person(string givenName, string surName, string idNum)
{
    public enum Gender
    {
        Male, Female
    }
    private (bool, Gender)? _idCheckCache;

    private static readonly Regex NamePatAux = new(@"[A-ZÄÖÅ]{1}[a-zåäö]+", RegexOptions.CultureInvariant);
    private static readonly Regex NamePat = new(@$"{NamePatAux}(-{NamePatAux})?", RegexOptions.CultureInvariant);
    // Allows spaced middle-names here
    // Escaping '-' due to some IDEs automatically showing '|' followed by '-' as a ligature
    public static readonly Regex GivenNamePat = new(@$"^{NamePat}( {NamePat})*$", RegexOptions.CultureInvariant);
    public static readonly Regex SurNamePat = new(@$"^{NamePat}$", RegexOptions.CultureInvariant);
    public static readonly Regex IdNumPat = new(@"^[0-9]{6}-?[0-9]{4}$");
    
    public override string ToString()
    {
        return String.Join(",", givenName, surName, idNum);
    }
    
    public (bool, Gender?) Validate()
    {
        bool[] regexResults = { GivenNamePat.IsMatch(givenName), SurNamePat.IsMatch(surName), IdNumPat.IsMatch(idNum) };
        // If any is False, exit early
        if (regexResults.Any(result => result == false))
        {
            return (false, null);
        }
        
        // Only call 'IdCheck' and assign if the cache is null
        _idCheckCache ??= IdCheck();
        
        // Return the cached value + the regex results
        return (
            GivenNamePat.IsMatch(givenName) && SurNamePat.IsMatch(surName) && IdNumPat.IsMatch(idNum) && _idCheckCache.Value.Item1,
            _idCheckCache.Value.Item2
        );
    }
    
    // Do both 21-algo and Control digit check
    private (bool, Gender) IdCheck()
    {
        // Left shift odd digits
        string tempIdNum = idNum;
        // Remove the dash if it exists
        if (tempIdNum.Length == 11)
        {
            tempIdNum = tempIdNum.Remove(6, 1);
        }
        
        // Add the odd digits
        int oddSum = 0;
        int controlSum = 0;
        for (int i = 0; i < 9; i += 2)
        {
            // Left shift the digit (multiply by 2) before adding
            oddSum += tempIdNum[i] << 1;
            // If the result is greater than 10, add the numbers together
            controlSum += tempIdNum[i] < 10 ? tempIdNum[i] : tempIdNum[i] / 10 + tempIdNum[i] % 10;
        }
        
        // Add even digits
        int evenSum = 0;
        for (int i = 1; i < 9; i += 2)
        {
            evenSum += tempIdNum[i];
            controlSum += tempIdNum[i] << 1;
        }
        
        // Add the two sums
        int sum = oddSum + evenSum;
        int controlDigit = 10 - (controlSum % 10);
        
        // Check if sum is divisible by 10
        // 9th letter is 3rd birth digit (8th index)
        Gender gender = int.Parse(tempIdNum[8].ToString()) % 2 == 0 ? Gender.Female : Gender.Male;
        return (sum % 10 == 0 && controlDigit == int.Parse(tempIdNum[9].ToString()), gender);
    }
}