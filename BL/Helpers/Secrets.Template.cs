namespace Helpers;

// Instructions for users downloading the code from Git:
// 1. Rename the class from SecretsTemplate to Secrets
// 2. Rename the file from Secrets.Template.cs to Secrets.cs
// 3. Insert your key in place of the text below

internal static class SecretsTemplate
{
    // public key for LocationIQ API, you can get it for free by signing up at https://locationiq.com/
    internal const string LocationIqApiKey = "YOUR_KEY_HERE";

    // email and app password for the email service, you can create an app password for free by
    // following the instructions at https://support.google.com/accounts/answer/185833?hl=en
    internal const string ServiceEmail = "YOUR_EMAIL_HERE";
    internal const string ServicePassword = "YOUR_APP_PASSWORD_HERE";
}