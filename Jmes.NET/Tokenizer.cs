using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
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
				case char c when char.IsAsciiDigit(c):
					tokens.Add((_index, ConsumeNumber()));
					break;
				case char c when char.IsWhiteSpace(c):
					// Ignore whitespace
					break;
				case '"':
					tokens.Add((_index, ConsumeQuotedIdentifier()));
					break;
				case '-':
					tokens.Add((_index, ConsumeNumber(true)));
					break;
				case '[':
					tokens.Add((_index, ConsumeBracket()));
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
				case '|':
					tokens.Add((_index, ConsumeMatchedOr<OrToken, PipeToken>('|')));
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
				case '=':
					MoveNext();
					switch (_enumerator.Current)
					{
						case '=':
							tokens.Add((_index, new EqToken()));
							break;
						default:
							throw new TokenizationException(
								$"Unexpected character '=' at index {_index}. Did you mean '=='?"
							);
					}
					break;
			}
		}

		tokens.Add((_index, new EofToken()));

		return tokens;
	}

	/// <summary>
	/// Consumes the next character if it matches the expected character, returning a token of the specified type.
	/// </summary>
	/// <typeparam name="TMatch">the type returned if the character matches</typeparam>
	/// <typeparam name="TNotMatch">the type returned if the character does not match</typeparam>
	/// <param name="toMatch">the character to match</param>
	/// <returns>an instance of either the matching or non-matching token type</returns>
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

	/// <summary>
	///	Consumes characters from the input while the specified predicate is true, starting from the current character in the input stream.
	/// </summary>
	/// <param name="predicate">a function to evaluate each character</param>
	/// <returns>a string containing the consumed characters</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string ConsumeWhile(Func<char, bool> predicate)
	{
		var builder = new StringBuilder();
		builder.Append(_enumerator.Current);

		while (_enumerator.Peek() is { } next && predicate(next))
		{
			MoveNext();
			builder.Append(_enumerator.Current);
		}

		return builder.ToString();
	}

	private string ConsumeUntil(char sentinel)
	{
		var startIndex = _index;
		var builder = new StringBuilder();

		while (MoveNext())
		{
			if (_enumerator.Current == sentinel)
			{
				return builder.ToString();
			}
			if (_enumerator.Current == '\\')
			{
				builder.Append(_enumerator.Current);
				if (!MoveNext())
				{
					throw new TokenizationException(
						$"Unterminated escape sequence starting at index {_index}"
					);
				}
				builder.Append(_enumerator.Current);
			}
		}

		throw new TokenizationException(
			$"Unterminated '{sentinel}' starting at index {startIndex}"
		);
	}

	private IdentifierToken ConsumeIdentifier()
	{
		var consumed = ConsumeWhile(c => char.IsAsciiLetter(c) || char.IsAsciiDigit(c) || c == '_');
		return new IdentifierToken(consumed);
	}

	private QuotedIdentifierToken ConsumeQuotedIdentifier()
	{
		var consumed = ConsumeUntil('"');
		return
			JsonNode.Parse($"\"{consumed}\"") is not JsonValue value
			|| value.GetValue<string>() is not string strValue
			? throw new TokenizationException(
				$"Invalid quoted identifier format at index {_index}: \"{consumed}\""
			)
			: new QuotedIdentifierToken(strValue);
	}

	private IToken ConsumeBracket()
	{
		switch (_enumerator.Peek())
		{
			case '?':
				MoveNext();
				return new FilterToken();
			case ']':
				MoveNext();
				return new FlattenToken();
			default:
				return new LBracketToken();
		}
	}

	private NumberToken ConsumeNumber(bool negative = false)
	{
		if ((negative && !MoveNext()) || !char.IsAsciiDigit(_enumerator.Current))
		{
			throw new TokenizationException(
				$"Invalid number format at index {_index}: '-' must be followed by a digit"
			);
		}

		var startingIndex = _index;
		var consumed = ConsumeWhile(char.IsAsciiDigit);
		try
		{
			var value = int.Parse(consumed);
			return new NumberToken(negative ? -value : value);
		}
		catch (FormatException)
		{
			throw new TokenizationException(
				$"Invalid number format at index {startingIndex}: {consumed}"
			);
		}
	}
}
