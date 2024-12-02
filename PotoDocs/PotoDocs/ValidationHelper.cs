using System.ComponentModel.DataAnnotations;

namespace PotoDocs;

public static class ValidationHelper
{
    /// <summary>
    /// Waliduje obiekt i zwraca błędy w postaci listy.
    /// </summary>
    /// <typeparam name="T">Typ obiektu do walidacji</typeparam>
    /// <param name="objectToValidate">Obiekt do walidacji</param>
    /// <returns>Lista błędów walidacji</returns>
    public static List<ValidationResult> Validate<T>(T objectToValidate)
    {
        var context = new ValidationContext(objectToValidate);
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(objectToValidate, context, results, true);

        return results;
    }

    /// <summary>
    /// Waliduje obiekt i zwraca błędy w postaci słownika.
    /// </summary>
    /// <typeparam name="T">Typ obiektu do walidacji</typeparam>
    /// <param name="objectToValidate">Obiekt do walidacji</param>
    /// <returns>Słownik błędów walidacji: klucz - właściwość, wartość - komunikat błędu</returns>
    public static Dictionary<string, string> ValidateToDictionary<T>(T objectToValidate)
    {
        var results = Validate(objectToValidate);
        return results
            .SelectMany(r => r.MemberNames, (result, memberName) => new { memberName, result.ErrorMessage })
            .ToDictionary(x => x.memberName, x => x.ErrorMessage ?? string.Empty);
    }
}
