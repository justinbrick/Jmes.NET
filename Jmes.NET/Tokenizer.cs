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

		while (MoveNext())
		{
			switch (_enumerator.Current)
			{
				case char c when char.IsAsciiLetter(c) || c == '_':
					tokens.Add((_index, ConsumeIdentifier()));
					break;
				case '.':
					tokens.Add((_index, new DotToken()));
					break;
				case '*':
					tokens.Add((_index, new StarToken()));
					break;
				case '@':
					tokens.Add((_index, new AtToken()));
					break;
				case '&':
					tokens.Add((_index, ConsumeMatchedOr<AndToken, AmpersandToken>('&')));
					break;
				case '>':
					tokens.Add((_index, ConsumeMatchedOr<GteToken, GtToken>('=')));
					break;
				case '<':
					tokens.Add((_index, ConsumeMatchedOr<LteToken, LtToken>('=')));
					break;
				case '!':
					tokens.Add((_index, ConsumeMatchedOr<NeqToken, NotToken>('=')));
					break;
				case '(':
					tokens.Add((_index, new LParenToken()));
					break;
				case ')':
					tokens.Add((_index, new RParenToken()));
					break;
				case ',':
					tokens.Add((_index, new CommaToken()));
					break;
				case ':':
					tokens.Add((_index, new ColonToken()));
					break;
				case ']':
					tokens.Add((_index, new RBracketToken()));
					break;
				case '{':
					tokens.Add((_index, new LBraceToken()));
					break;
				case '}':
					tokens.Add((_index, new RBraceToken()));
					break;
			}
		}

		tokens.Add((_index, new EofToken()));

		return tokens;
	}

	private IToken ConsumeMatchedOr<TMatch, TNotMatch>(char toMatch)
		where TMatch : IToken, new()
		where TNotMatch : IToken, new()
	{
		if (_enumerator.Peek() == toMatch)
		{
			MoveNext();
			return new TMatch();
		}
		return new TNotMatch();
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
