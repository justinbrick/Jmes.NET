namespace Jmes.NET;

public sealed class TokenizationException : Exception
{
	public TokenizationException() : base() { }
	public TokenizationException(string message) : base(message) { }
	public TokenizationException(string message, Exception inner) : base(message, inner) { }
}

internal static class Tokenizer
{
	public static void Tokenize()
	{
	}
}
