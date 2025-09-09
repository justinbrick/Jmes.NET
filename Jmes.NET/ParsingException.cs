using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TokenIndex = (long, Jmes.NET.JmesToken);

namespace Jmes.NET;

public sealed class ParsingException : Exception
{
	public ParsingException()
		: base() { }

	public ParsingException(string message)
		: base(message) { }

	public ParsingException(string message, Exception inner)
		: base(message, inner) { }
}
