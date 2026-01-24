namespace PotoDocs.Shared.Utils;

public static class NumberToWordsConverter
{
    public static string AmountInWords(decimal amount, string currency)
    {
        string prefix = "";
        if (amount < 0)
        {
            prefix = "minus ";
            amount = Math.Abs(amount);
        }

        string[] currencyForms;
        if (currency.Equals("EUR", StringComparison.CurrentCultureIgnoreCase))
        {
            currencyForms = ["euro", "euro", "euro"];
        }
        else
        {
            currencyForms = ["złotych", "złoty", "złote"];
        }

        long integerPart = (long)amount;

        long decimalPart = (long)Math.Round((amount - integerPart) * 100);

        string result;

        if (integerPart == 0)
        {
            result = "zero " + currencyForms[0];
        }
        else
        {
            result = NumberToWords(integerPart) + " " + CurrencyForm(integerPart, currencyForms);
        }

        result += $" {decimalPart:00}/100";

        return (prefix + result).Trim();
    }

    private static string NumberToWords(long number)
    {
        if (number == 0)
            return "zero";

        string[] units = ["", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć"];
        string[] teens = ["dziesięć", "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście"];
        string[] tens = ["", "dziesięć", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt"];
        string[] hundreds = ["", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset"];

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

        return result.Trim();
    }

    private static string GetGrammaticalForm(long number, string form1, string form2_4, string formOther)
    {
        if (number == 1) return form1;

        long mod10 = number % 10;
        long mod100 = number % 100;

        if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20))
        {
            return form2_4;
        }

        return formOther;
    }

    private static string CurrencyForm(long number, string[] forms)
    {
        return GetGrammaticalForm(number, forms[1], forms[2], forms[0]);
    }

    private static string ThousandForm(long number)
    {
        return GetGrammaticalForm(number, "tysiąc", "tysiące", "tysięcy");
    }
}