namespace OpenAi.question;

public static class OpenAIStringUtilities
{
    public static string Optimize(this string input) => input.ReplaceLineEndings(" ");
}