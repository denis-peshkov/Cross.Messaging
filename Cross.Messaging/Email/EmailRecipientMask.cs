namespace Cross.Messaging.Email;

/// <summary>
/// Masks email addresses for logging and exception messages.
/// </summary>
internal static class EmailRecipientMask
{
    /// <summary>
    /// Masks an email address, keeping the first local character and the full domain.
    /// </summary>
    /// <param name="email">Email address to mask.</param>
    /// <returns>Masked value safe for logs.</returns>
    public static string Mask(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return email;
        }

        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0 || atIndex == trimmed.Length - 1)
        {
            return "***";
        }

        var local = trimmed.Substring(0, atIndex);
        var domain = trimmed.Substring(atIndex);
        if (local.Length == 1)
        {
            return "*" + domain;
        }

        return local[0] + new string('*', local.Length - 1) + domain;
    }
}
