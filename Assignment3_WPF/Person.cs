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
        string tempIdStr = idNum;
        // Remove the dash if it exists
        if (tempIdStr.Length == 11) tempIdStr = tempIdStr.Remove(6, 1);
        // Left shift odd digits
        List<int> tempIdNum = tempIdStr.Select(c => int.Parse(c.ToString())).ToList();
        
        // Multiply every other digit by 2
        for (int i = 0; i < 9; i += 2)
        {
            // Left shift the digit (multiply by 2)
            tempIdNum[i] <<= 1;
            
            // If the result is greater than 10, add the numbers together
            // -9 is equivalent to the sum of the digits in the range [10, 18]
            tempIdNum[i] = tempIdNum[i] > 9 ? tempIdNum[i] - 9 : tempIdNum[i];
        }
        
        // Skip even number operation because they are all identity operations
        
        // Calculate the (even, odd) sum
        int sum = tempIdNum.Sum();
        // The control sum is the sum without the last digit
        int controlSum = sum - tempIdNum[9];
        // The control digit is the number that makes the control sum divisible by 10
        int controlDigit = 10 - (controlSum % 10);
        
        // Check if sum is divisible by 10
        // 9th letter is 3rd birth digit (8th index)
        Gender gender = tempIdNum[8] % 2 == 0 ? Gender.Female : Gender.Male;
        return (sum % 10 == 0 && controlDigit == tempIdNum[9], gender);
    }
}