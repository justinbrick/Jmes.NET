using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TokenIndex = (long, Jmes.NET.JmesToken);

namespace Jmes.NET;

public sealed class TokenizationException : Exception
{
	public TokenizationException()
		: base() { }

	public TokenizationException(string message)
		: base(message) { }

	public TokenizationException(string message, Exception inner)
		: base(message, inner) { }
}
