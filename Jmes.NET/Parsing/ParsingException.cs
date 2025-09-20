namespace Jmes.NET.Parsing;

public sealed class ParsingException : Exception
{
	public ParsingException()
		: base() { }

	public ParsingException(string message)
		: base(message) { }

	public ParsingException(string message, Exception inner)
		: base(message, inner) { }
}
