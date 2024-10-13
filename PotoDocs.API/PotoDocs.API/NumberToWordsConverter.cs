namespace PotoDocs.API;

public static class NumberToWordsConverter
{
    public static string AmountInWords(decimal amount, string currency)
    {
        string[] currencyForms = currency == "EUR" ? new[] { "euros", "euro", "euros" } : new[] { "zlotych", "zloty", "zloty" };

        long integerPart = (long)Math.Floor(amount);
        long decimalPart = (long)((amount - integerPart) * 100);

        string result = "";

        if (integerPart == 0)
        {
            result = "zero " + currencyForms[0];
        }
        else
        {
            result = NumberToWords(integerPart) + " " + CurrencyForm(integerPart, currencyForms);
        }

        result += $" {decimalPart:00}/100";

        return result;
    }

    private static string NumberToWords(long number)
    {
        string[] units = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        string[] teens = { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
        string[] tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
        string[] hundreds = { "", "one hundred", "two hundred", "three hundred", "four hundred", "five hundred", "six hundred", "seven hundred", "eight hundred", "nine hundred" };

        string result = "";

        if (number >= 1000)
        {
            long thousands = number / 1000;
            result += NumberToWords(thousands) + " " + ThousandForm(thousands);
            number %= 1000;
        }

        if (number >= 100)
        {
            result += (result == "" ? "" : " ") + hundreds[number / 100];
            number %= 100;
        }

        if (number >= 20)
        {
            result += (result == "" ? "" : " ") + tens[number / 10];
            number %= 10;
        }

        if (number >= 10)
        {
            result += (result == "" ? "" : " ") + teens[number - 10];
        }
        else if (number > 0)
        {
            result += (result == "" ? "" : " ") + units[number];
        }

        return result;
    }

    private static string CurrencyForm(long number, string[] forms)
    {
        if (number == 1) return forms[1];
        if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 >= 20)) return forms[2]; // euros/zloty (2-4)
        return forms[0];
    }

    private static string ThousandForm(long number)
    {
        if (number == 1) return "thousand";
        if (number % 10 >= 2 && number % 10 <= 4 && (number % 100 < 10 || number % 100 >= 20)) return "thousands";
        return "thousand";
    }
}
