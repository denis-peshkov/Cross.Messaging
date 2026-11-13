namespace Cross.Messaging.Masking;

/// <summary>
/// Masks phone numbers for logging and exception messages.
/// </summary>
internal static class PhoneDestinationMask
{
    private const int VisibleSuffixLength = 4;

    /// <summary>
    /// Masks a destination phone number, keeping the calling code and last digits.
    /// Accepts E.164 (<c>+4072…</c>) and digits-only (<c>4072…</c>) forms.
    /// </summary>
    /// <param name="destination">Phone number to mask.</param>
    /// <returns>Masked value safe for logs.</returns>
    public static string Mask(string destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return destination;
        }

        var trimmed = NormalizeToE164(destination);
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

    /// <summary>
    /// Masks <paramref name="value" /> when it looks like a phone number; otherwise returns it unchanged.
    /// </summary>
    /// <param name="value">Candidate phone number or diagnostic key (for example <c>src_addr</c>).</param>
    /// <returns>Masked phone or the original diagnostic key.</returns>
    public static string MaskIfPhone(string value)
    {
        if (!LooksLikePhoneNumber(value))
        {
            return value;
        }

        return Mask(value);
    }

    /// <summary>
    /// Normalizes a phone value to E.164 form by prefixing <c>+</c> when the input is digits-only.
    /// </summary>
    /// <param name="value">Phone number in E.164 or digits-only form.</param>
    /// <returns>E.164-style value when possible; otherwise the trimmed original.</returns>
    public static string NormalizeToE164(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("+", StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (trimmed.Length > 0 && trimmed.All(char.IsDigit))
        {
            return "+" + trimmed;
        }

        return trimmed;
    }

    /// <summary>
    /// Returns whether <paramref name="value" /> looks like a phone number (optional <c>+</c> followed by digits).
    /// </summary>
    /// <param name="value">Candidate phone number or other key.</param>
    /// <returns><see langword="true" /> when the value is phone-like; otherwise <see langword="false" />.</returns>
    public static bool LooksLikePhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var digits = value.Trim();
        if (digits.StartsWith("+", StringComparison.Ordinal))
        {
            digits = digits.Substring(1);
        }

        return digits.Length > 0 && digits.All(char.IsDigit);
    }
}
