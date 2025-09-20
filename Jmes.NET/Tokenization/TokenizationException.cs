using TokenIndex = (long, Jmes.NET.Tokenization.JmesToken);

namespace Jmes.NET.Tokenization;

public sealed class TokenizationException : Exception
{
	public TokenizationException()
		: base() { }

	public TokenizationException(string message)
		: base(message) { }

	public TokenizationException(string message, Exception inner)
		: base(message, inner) { }
}
