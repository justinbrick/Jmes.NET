using System.Text;
using TokenIndex = (ulong, Jmes.NET.IToken);

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

public sealed class Tokenizer
{
	private readonly PeekableEnumerator<char> _enumerator;
	private ulong _index = 0;

	public static List<TokenIndex> Tokenize(IEnumerable<char> enumerable)
	{
		var tokenizer = new Tokenizer(enumerable);
		return tokenizer.Tokenize();
	}

	public Tokenizer(IEnumerable<char> enumerable)
	{
		_enumerator = new PeekableEnumerator<char>(enumerable.GetEnumerator());
	}

	public Tokenizer(IEnumerator<char> enumerator)
	{
		_enumerator = new PeekableEnumerator<char>(enumerator);
	}

	private bool MoveNext()
	{
		var moved = _enumerator.MoveNext();
		if (moved)
		{
			_index++;
		}
		return moved;
	}

	public List<TokenIndex> Tokenize()
	{
		var tokens = new List<TokenIndex>();
		do
		{
			switch (_enumerator.Current)
			{
				case char c when char.IsAsciiLetter(c) || c == '_':
					tokens.Add((_index, ConsumeIdentifier()));
					break;
			}
		} while (MoveNext());

		tokens.Add((_index, new EofToken()));

		return tokens;
	}

	private IdentifierToken ConsumeIdentifier()
	{
		var builder = new StringBuilder();
		builder.Append(_enumerator.Current);

		while (
			_enumerator.Peek() is { } next
			&& (char.IsAsciiLetter(next) || char.IsAsciiDigit(next) || next == '_')
		)
		{
			MoveNext();
			builder.Append(_enumerator.Current);
		}

		return new IdentifierToken { Value = builder.ToString() };
	}
}
