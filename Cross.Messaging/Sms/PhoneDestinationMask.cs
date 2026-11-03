namespace Cross.Messaging.Sms;

/// <summary>
/// Masks E.164 phone numbers for logging.
/// </summary>
internal static class PhoneDestinationMask
{
    private const int VisibleSuffixLength = 4;

    /// <summary>
    /// Masks a destination phone number, keeping the calling code and last digits.
    /// </summary>
    /// <param name="destination">Phone number to mask.</param>
    /// <returns>Masked value safe for logs.</returns>
    public static string Mask(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return destination;
        }

        var trimmed = destination.Trim();
        if (PhoneCountryResolver.TryResolve(trimmed, out _, out var callingCode))
        {
            var digits = trimmed.Substring(1);
            var national = digits.Substring(callingCode.Length);
            if (national.Length <= VisibleSuffixLength)
            {
                return "+" + callingCode + new string('*', national.Length);
            }

            var suffix = national.Substring(national.Length - VisibleSuffixLength);
            var maskedLength = national.Length - VisibleSuffixLength;
            return "+" + callingCode + new string('*', maskedLength) + suffix;
        }

        if (trimmed.StartsWith("+", StringComparison.Ordinal) && trimmed.Length > VisibleSuffixLength + 1)
        {
            var suffix = trimmed.Substring(trimmed.Length - VisibleSuffixLength);
            return "+" + new string('*', trimmed.Length - 1 - VisibleSuffixLength) + suffix;
        }

        return new string('*', Math.Min(trimmed.Length, 8));
    }
}
