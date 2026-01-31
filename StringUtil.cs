using System.Collections.Generic;
using System;

public static class StringUtil
{
    public static string ToUpperFirst(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        char[] array = text.ToCharArray();
        array[0] = char.ToUpper(array[0]);
        return new string(array);
    }

    public static bool ContainsIgnoreCase(this string text, string value)
    {
        if (text == null) return false;
        return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool EqualsIgnoreCase(this string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Converts a CamelCase, PascalCase, or snake_case string into a spaced, capitalized format.
    /// </summary>
    /// <param name="text">The string to format.</param>
    /// <returns>A "nicified" version of the string.</returns>
    public static string NicifyString(this string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Replace underscores with spaces
        string result = text.Replace("_", " ");
        
        // Insert spaces before capital letters (e.g., CamelCase to Camel Case)
        result = System.Text.RegularExpressions.Regex.Replace(result, "([a-z])([A-Z])", "$1 $2");

        // Ensure first letter is capitalized
        return char.ToUpper(result[0]) + result.Substring(1);
    }

    /// <summary>
    /// Performs a natural-string comparison:
    /// - Alphabetical comparison for non-numeric characters.
    /// - Any contiguous digit/decimal run is parsed as a number and compared numerically.
    /// - Numbers with equal numeric value are ordered by literal length (more leading zeros come after).
    /// - After a numeric block, comparison resumes lexically.
    /// </summary>
    public static int NaturalCompare(string left, string right)
    {
        if (left == null) return right == null ? 0 : -1;
        if (right == null) return 1;

        int leftIndex = 0;
        int rightIndex = 0;
        int leftLength = left.Length;
        int rightLength = right.Length;

        while (leftIndex < leftLength && rightIndex < rightLength)
        {
            bool leftIsNumeric =
                char.IsDigit(left[leftIndex]) ||
                (left[leftIndex] == '.' && leftIndex + 1 < leftLength && char.IsDigit(left[leftIndex + 1]));

            bool rightIsNumeric =
                char.IsDigit(right[rightIndex]) ||
                (right[rightIndex] == '.' && rightIndex + 1 < rightLength && char.IsDigit(right[rightIndex + 1]));

            // numeric block comparison
            if (leftIsNumeric && rightIsNumeric)
            {
                int leftStart = leftIndex;
                int rightStart = rightIndex;

                // consume numeric block (digits + decimal point)
                while (leftIndex < leftLength && (char.IsDigit(left[leftIndex]) || left[leftIndex] == '.'))
                    leftIndex++;

                while (rightIndex < rightLength && (char.IsDigit(right[rightIndex]) || right[rightIndex] == '.'))
                    rightIndex++;

                string leftNumericText = left.Substring(leftStart, leftIndex - leftStart);
                string rightNumericText = right.Substring(rightStart, rightIndex - rightStart);

                bool leftParsed = decimal.TryParse(leftNumericText, out decimal leftValue);
                bool rightParsed = decimal.TryParse(rightNumericText, out decimal rightValue);

                // fallback to lexical if parsing fails for either side
                if (!leftParsed || !rightParsed)
                {
                    return string.Compare(leftNumericText, rightNumericText, StringComparison.Ordinal);
                }

                int numericComparison = leftValue.CompareTo(rightValue);
                if (numericComparison != 0)
                    return numericComparison;

                // identical numeric value ? longer literal sorts after shorter
                int lengthComparison = leftNumericText.Length.CompareTo(rightNumericText.Length);
                if (lengthComparison != 0)
                    return lengthComparison;

                continue;
            }

            // non-numeric char compare
            int charComparison = left[leftIndex].CompareTo(right[rightIndex]);
            if (charComparison != 0)
                return charComparison;

            leftIndex++;
            rightIndex++;
        }

        // shorter string sorts first
        return leftLength.CompareTo(rightLength);
    }

    /// <summary>
    /// Sorts a list of strings using natural compare function.
    /// </summary>
    /// <param name="list">list to be sorted</param>
    public static void NaturalSort(this List<string> list)
    {
        if (list == null) return;
        list.Sort(NaturalCompare);
    }

    /// <summary>
    /// Concatenates the string representations of the elements in a sequence,
    /// using the provided function to convert each element to a string, and inserting the
    /// specified separator between each converted string.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="items">The sequence of elements to join.</param>
    /// <param name="customToString">A function that converts each element into a string.</param>
    /// <param name="separator">The string to use as a separator. Default is ", ".</param>
    /// <returns>
    /// A single concatenated string of the selected values, separated by the
    /// specified separator. Returns an empty string if <paramref name="items"/> is empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="items"/>, <paramref name="customToString"/>, or 
    /// <paramref name="separator"/> is null.
    /// </exception>
    /// <exception cref="Exception">
    /// Any exception thrown by <paramref name="customToString"/> for an element will propagate.
    /// </exception>
    public static string Join<T>(IEnumerable<T> items, System.Func<T, string> customToString, string separator = ", ")
    {
        System.Text.StringBuilder sb = new();
        bool first = true;

        foreach (T item in items)
        {
            if (!first) sb.Append(separator);
            sb.Append(customToString(item));
            first = false;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Allows an object reference, that may be null, to generate a string, without error or exceptions being thrown.
    /// </summary>
    /// <param name="obj">reference to object that should have to string run on it.</param>
    /// <returns></returns>
    static public string SafeToString(this object obj)
    {
        if (obj == null) return "null";
        return obj.ToString();
    }

    /// <summary>
    /// Generates a text table as output from a 2d array of data.  Similar to the string.Join function
    /// </summary>
    /// <typeparam name="T">type of data to be displayed.  The virtual object.ToString function (with no parameters) for the type T will be invoked to generate the output for each data element.</typeparam>
    /// <param name="dataArray">a 2D array of T's.  Contain the data that will be displayed.</param>
    /// <param name="seperator">text to be displayed between each element on the same line.  default value is a tab.</param>
    /// <param name="includeHeaders">Specified weather the header (showing "Column X") should be displayed at the top of all columns</param>
    /// <param name="linePrepend">string to be added to the begining of each line in a table-0  this can be useful for indenting tables.</param>
    /// <param name="lineAppend">string to be appended to each line of the table.</param>
    /// <param name="customToString">optional function that will be used to generate a string from an object of type T.  If left off, or null is passed, the default ToString() function for T will be used.</param>
    /// <returns>the generated table as a single string</returns>
    public static string GenerateStringTable<T>(T[,] dataArray, string seperator = "\t", bool includeHeaders = false, string linePrepend = "", string lineAppend = "", System.Func<T, string> customToString=null)
    {
        int rows = dataArray.GetLength(0);
        int columns = dataArray.GetLength(1);

        // String builder to construct the table
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Optionally add headers
        if (includeHeaders)
        {
            sb.Append(linePrepend);
            for (int j = 0; j < columns; j++)
            {
                sb.Append("Column " + (j + 1) + seperator);
            }
            sb.Append(lineAppend);
            sb.AppendLine();
        }

        // Add data rows
        for (int i = 0; i < rows; i++)
        {
            sb.Append(linePrepend);
            for (int j = 0; j < columns; j++)
            {
                if(customToString==null)
                    sb.Append(dataArray[i, j].ToString() + seperator);
                else
                    sb.Append(customToString(dataArray[i, j]) + seperator); 
            }
            sb.Append(lineAppend);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Scans the list for any non-null string that contains the specified substring,
    /// using the provided comparison mode. Returns true on the first match.
    /// </summary>
    public static bool ContainsSubString(this List<string> stringList, string subString, StringComparison compareType = StringComparison.Ordinal)
    {
        foreach (string listElement in stringList)
        {
            if (listElement != null && listElement.Contains(subString, compareType))
                return true;
        }
        return false;
    }

    private const string singleQuote = "\"";
    private const string escapedQuote = "\\\"";

    /// <summary>
    /// Places a quote around a given string. Converts each single quote contained in the passed in string into "escaped quotes".
    /// </summary>
    /// <param name="rawString"></param>
    /// <returns></returns>
    public static string Quote(string rawString)
    {
        return singleQuote + rawString.Replace(singleQuote, escapedQuote) + singleQuote;
    }


    /// <summary>
    /// Removes surrounding double quotes from a string, if present, 
    /// and converts any escaped quotes (\") inside back to normal quotes.
    /// If the string is not properly quoted, returns the original string unchanged.
    /// </summary>
    /// <param name="quotedString">The string that may be surrounded by quotes.</param>
    /// <returns>The unquoted string with escaped quotes restored.</returns>
    public static string UnQuote(string quotedString)
    {
        string trimmed = quotedString.Trim();
        if (trimmed.Length >= 2 &&
            trimmed[0] == '"' &&
            trimmed[trimmed.Length - 1] == '"')
        {
            string inner = trimmed.Substring(1, trimmed.Length - 2);
            return inner.Replace(escapedQuote, singleQuote);
        }
        return quotedString;
    }


    public static string UnBracket(string bracketedString)
    {
        string trimmed = bracketedString.Trim();
        if (trimmed.Length >= 2 &&
            trimmed[0] == '{' &&
            trimmed[trimmed.Length - 1] == '}')
        {
            string inner = trimmed.Substring(1, trimmed.Length - 2);
            return inner.Replace(escapedQuote, singleQuote);
        }
        return bracketedString;
    }



    static System.Random random = new System.Random();
    static public List<string> syllables = new List<string>
    {
        "abri", "aco", "ad", "bal", "ben", "ca", "lor", "da", "de", "fa",
        "fe", "ga", "ge", "ha", "he", "ja", "je", "ka", "ke", "la",
        "lem", "ma", "me", "nab", "nel", "pa", "pe", "rab", "re", "jef",
        "pan", "ta", "del", "va", "ve", "wa", "we", "da", "kal", "ya",
        "ye", "tor", "pel"
    };

    static public string GenerateRandomName(int syllableCount = -1)
    {
        if (syllableCount == -1)
            syllableCount = random.Next(2, 3); // Generate a name with 2 to 3 syllables
        List<string> nameParts = new List<string>();


        for (int i = 0; i < syllableCount; i++)
        {
            int syllableIndex = random.Next(syllables.Count);
            nameParts.Add(syllables[syllableIndex]);
        }

        return string.Concat(nameParts).ToUpperFirst();//.Select(part => part.ToUpperFirst()));
    }
}