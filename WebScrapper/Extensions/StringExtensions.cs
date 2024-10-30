using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace WebScrapper.Services.Extensions;

public static class StringExtensions
{
    public static decimal GetPrice(this string priceString)
    {
        // Step 1: Extract only numeric parts (including dots and commas)
        var numericString = Regex.Match(priceString, @"[\d.,]+").Value;

        if (string.IsNullOrEmpty(numericString))
        {
            throw new FormatException("No numeric value found in the price string.");
        }

        // Step 2: Standardize decimal format (replace comma with dot)
        numericString = numericString.Replace(",", ".");

        return decimal.Parse(numericString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
    }

    public static int GetId(this string url)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] bytes = Encoding.UTF8.GetBytes(url);
        byte[] hash = sha256.ComputeHash(bytes);

        // Use the first 4 bytes of the hash to generate a numeric value
        int numericId = BitConverter.ToInt32(hash, 0);

        // Ensure the ID is positive and within 6 digits
        numericId = Math.Abs(numericId) % 1000000;

        return numericId;
    }
}
