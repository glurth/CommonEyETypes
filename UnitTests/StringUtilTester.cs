using System.Collections.Generic;
using System.Text;
using System;
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public static class UnityTester
{
    [MenuItem("Tests/StringUtilTests")]
    public static void UnityRunTestsToConsole()
    {
        StringUtilTester tester = new StringUtilTester();
        string report = tester.RunAllTests();
        Debug.Log(report);
    }
}
#endif
public class StringUtilTester
{
    private StringBuilder _report = new StringBuilder();
    private bool _allPassed = true;
    private int _passCount = 0;
    private int _failCount = 0;

    public string RunAllTests()
    {
        _report.Clear();
        _allPassed = true; _passCount = 0; _failCount = 0;

        _report.AppendLine("==================================================");
        _report.AppendLine("         STRINGUTIL VERIFICATION REPORT           ");
        _report.AppendLine("==================================================");

        TestToUpperFirst();
        TestNicifyString();
        TestIncrementTrailingInteger();
        TestNaturalCompare();
        TestJoinExceptions(); // New!
        TestRandomNames();

        _report.AppendLine("==================================================");
        _report.AppendLine(_allPassed ? "? RESULT: ALL PASSED" : "? RESULT: NOT ALL PASSED");
        _report.AppendLine($"Total: {_passCount + _failCount} | Passed: {_passCount} | Failed: {_failCount}");
        _report.AppendLine("==================================================");

        return _report.ToString();
    }

    // --- TO UPPER FIRST ---
    private static readonly string UpperIn1 = "123numbers";
    private static readonly string UpperIn2 = " mixed Case";
    private void TestToUpperFirst()
    {
        _report.AppendLine("\n[SECTION: ToUpperFirst]");
        CheckOutput("Numbers (No change)", UpperIn1, UpperIn1.ToUpperFirst(), "123numbers");
        CheckOutput("Leading Space (No change)", UpperIn2, UpperIn2.ToUpperFirst(), " mixed Case");
    }

    // --- NICIFY STRING ---
    private static readonly string NicifyIn1 = "APIControllerFactory";
    private static readonly string NicifyIn2 = "snake_case_with_Numbers123";
    private static readonly string NicifyIn3 = "Already Nicified String";
    private void TestNicifyString()
    {
        _report.AppendLine("\n[SECTION: NicifyString]");
        // Tricky: handling multiple capitals in a row
        CheckOutput("Acronyms", NicifyIn1, NicifyIn1.NicifyString(), "API Controller Factory");
        CheckOutput("Snake with Numbers", NicifyIn2, NicifyIn2.NicifyString(), "Snake case with Numbers123");
        CheckOutput("Passthrough", NicifyIn3, NicifyIn3.NicifyString(), "Already Nicified String");
    }

    // --- INCREMENT INTEGER ---
    private static readonly string IncIn1 = "99";
    private static readonly string IncIn2 = "File-001";
    private static readonly string IncIn3 = "9223372036854775807"; // long.MaxValue
    private void TestIncrementTrailingInteger()
    {
        _report.AppendLine("\n[SECTION: IncrementTrailingInteger]");
        CheckOutput("Roll over 99", IncIn1, IncIn1.IncrementTrailingInteger(), "100");
        CheckOutput("Negative-lookalike", IncIn2, IncIn2.IncrementTrailingInteger(), "File-2"); // Note: treats '-' as prefix

        // This tests the limit you noted earlier
        _report.AppendLine("  > Note: Testing near 64-bit limit...");
        CheckOutput("Max Long", IncIn3, IncIn3.IncrementTrailingInteger(), IncIn3); // Should fallback/fail gracefully
    }

    // --- NATURAL COMPARE ---
    private static readonly string NatL1 = "1.0.2"; private static readonly string NatR1 = "1.0.10";
    private static readonly string NatL2 = "image_2.png"; private static readonly string NatR2 = "image_11.png";
    private static readonly string NatL3 = "001"; private static readonly string NatR3 = "1";
    private void TestNaturalCompare()
    {
        _report.AppendLine("\n[SECTION: NaturalCompare]");
        CheckOutput("Version Dots", "1.0.2 vs 1.0.10", StringUtil.NaturalCompare(NatL1, NatR1) < 0, true);
        CheckOutput("Filenames", "img_2 vs img_11", StringUtil.NaturalCompare(NatL2, NatR2) < 0, true);
        CheckOutput("Leading Zeros", "001 vs 1", StringUtil.NaturalCompare(NatL3, NatR3) > 0, true); // Identical value, longer string sorts after
    }

    // --- EXCEPTION TESTING ---
    private void TestJoinExceptions()
    {
        _report.AppendLine("\n[SECTION: Exception Handling]");

        // Testing ArgumentNullException in StringUtil.Join
        CheckException<ArgumentNullException>("Join with null items", () => {
            StringUtil.Join<string>(null, s => s);
        });

        CheckException<ArgumentNullException>("Join with null converter", () => {
            StringUtil.Join(new List<string> { "a" }, null);
        });
    }

    // --- RANDOM NAMES ---
    private void TestRandomNames()
    {
        _report.AppendLine("\n[SECTION: RandomName]");
        // Test syllable count boundary
        CheckOutput("0 Syllables", 0, StringUtil.GenerateRandomName(0), "");
    }

    // --- HELPERS ---

    private void CheckOutput<T>(string testName, object input, T actual, T expected)
    {
        bool passed = EqualityComparer<T>.Default.Equals(actual, expected);
        LogResult(passed, testName, input, actual, expected);
    }

    private void CheckException<T>(string testName, Action action) where T : Exception
    {
        bool caughtCorrect = false;
        Exception caught = null;
        try
        {
            action();
        }
        catch (T)
        {
            caughtCorrect = true;
        }
        catch (Exception e)
        {
            caught = e;
        }

        if (caughtCorrect) _passCount++; else _failCount++;
        if (!caughtCorrect) _allPassed = false;

        string status = caughtCorrect ? "[PASS]" : "[FAIL]";
        _report.AppendLine($"{status} {testName} (Expected Exception: {typeof(T).Name})");
        if (!caughtCorrect && caught != null)
            _report.AppendLine($"       Actual Exception: {caught.GetType().Name}");
    }

    private void LogResult<T>(bool passed, string testName, object input, T actual, T expected)
    {
        if (passed) _passCount++; else _failCount++;
        if (!passed) _allPassed = false;

        _report.AppendLine($"{(passed ? "[PASS]" : "[FAIL]")} {testName}");
        _report.AppendLine($"       In:  {input.SafeToString()}");
        _report.AppendLine($"       Exp: {expected.SafeToString()}");
        _report.AppendLine($"       Act: {actual.SafeToString()}");
    }
}