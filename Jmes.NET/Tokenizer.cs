using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using TokenIndex = (long, Jmes.NET.JmesToken);

namespace Jmes.NET;

public static class Tokenizer
{
	public static List<TokenIndex> Tokenize<T>(T enumerator)
		where T : IEnumerator<char>
	{
		Tokenizer<T> tokenizer = new(enumerator);
		return tokenizer.Tokenize();
	}

	public static List<TokenIndex> Tokenize(string input)
	{
		return Tokenize(input.GetEnumerator());
	}
}

public sealed class Tokenizer<T>(T enumerator)
	where T : IEnumerator<char>
{
	private readonly PeekableEnumerator<T, char> _enumerator = new(enumerator);
	private long _index = -1;

	private bool MoveNext()
	{
		_index++;
		return _enumerator.MoveNext();
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
				case '`':
					tokens.Add((_index, ConsumeLiteral()));
					break;
				case '-':
					tokens.Add((_index, ConsumeNumber(true)));
					break;
				case '[':
					tokens.Add((_index, ConsumeBracket()));
					break;
				case '.':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.Dot)));
					break;
				case '*':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.Star)));
					break;
				case '@':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.At)));
					break;
				case '&':
					tokens.Add(
						(_index, ConsumeMatchedOr(JmesTokenType.And, JmesTokenType.Ampersand, '&'))
					);
					break;
				case '>':
					tokens.Add(
						(_index, ConsumeMatchedOr(JmesTokenType.Gte, JmesTokenType.Gt, '='))
					);
					break;
				case '<':
					tokens.Add(
						(_index, ConsumeMatchedOr(JmesTokenType.Lte, JmesTokenType.Lt, '='))
					);
					break;
				case '!':
					tokens.Add(
						(_index, ConsumeMatchedOr(JmesTokenType.Neq, JmesTokenType.Not, '='))
					);
					break;
				case '|':
					tokens.Add(
						(_index, ConsumeMatchedOr(JmesTokenType.Or, JmesTokenType.Pipe, '|'))
					);
					break;
				case '(':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.LParen)));
					break;
				case ')':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.RParen)));
					break;
				case ',':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.Comma)));
					break;
				case ':':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.Colon)));
					break;
				case ']':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.RBracket)));
					break;
				case '{':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.LBrace)));
					break;
				case '}':
					tokens.Add((_index, JmesToken.Make(JmesTokenType.RBrace)));
					break;
				case '=':
					if (!MoveNext())
					{
						throw new TokenizationException(
							$"Unexpected end of input for '=' at index {_index - 1}"
						);
					}
					switch (_enumerator.Current)
					{
						case '=':
							tokens.Add((_index - 1, JmesToken.Make(JmesTokenType.Eq)));
							break;
						default:
							throw new TokenizationException(
								$"Unexpected character '=' at index {_index}. Did you mean '=='?"
							);
					}
					break;
				default:
					throw new TokenizationException(
						$"Unexpected character '{_enumerator.Current}' at index {_index}"
					);
			}
		}

		tokens.Add((_index, JmesToken.Make(JmesTokenType.Eof)));

		return tokens;
	}

	/// <summary>
	/// Consumes the next character if it matches the expected character, returning a token of the specified type.
	/// </summary>
	/// <param name="toMatch">the character to match</param>
	/// <returns>a <see cref="JmesToken"/> of either the matching or non-matching token type</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private JmesToken ConsumeMatchedOr(
		JmesTokenType matchType,
		JmesTokenType notMatchType,
		char toMatch
	)
	{
		if (_enumerator.Peek() == toMatch)
		{
			MoveNext();
			return JmesToken.Make(matchType);
		}
		return JmesToken.Make(notMatchType);
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

	/// <summary>
	///	Consumes characters until the specified sentinel character is reached.
	/// </summary>
	/// <param name="sentinel">the character to terminate consumption</param>
	/// <returns>a string containing the consumed characters</returns>
	/// <exception cref="TokenizationException">unexpected end of input</exception>
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

			builder.Append(_enumerator.Current);

			if (_enumerator.Current == '\\')
			{
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

	/// <summary>
	/// Consumes an identifier from the input stream.
	/// </summary>
	/// <returns>An <see cref="IdentifierToken"/> representing the consumed identifier.</returns>
	private JmesToken ConsumeIdentifier()
	{
		var consumed = ConsumeWhile(c => char.IsAsciiLetter(c) || char.IsAsciiDigit(c) || c == '_');
		return JmesToken.Identifier(consumed);
	}

	/// <summary>
	/// Consumes a quoted identifier from the input stream.
	/// </summary>
	/// <returns>A <see cref="QuotedIdentifierToken"/> representing the consumed quoted identifier.</returns>
	/// <exception cref="TokenizationException">the input is not a valid quoted identifier</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private JmesToken ConsumeQuotedIdentifier()
	{
		var consumed = ConsumeUntil('"');
		return
			JsonNode.Parse($"\"{consumed}\"") is not JsonValue value
			|| value.GetValue<string>() is not string strValue
			? throw new TokenizationException(
				$"Invalid quoted identifier format at index {_index}: \"{consumed}\""
			)
			: JmesToken.QuotedIdentifier(strValue);
	}

	/// <summary>
	///	Consumes a literal JSON object inside of ticks (`)
	/// </summary>
	/// <returns>A <see cref="JmesToken"/> representing the consumed literal.</returns>
	/// <exception cref="TokenizationException"></exception>
	private JmesToken ConsumeLiteral()
	{
		var start = _index;
		var consumed = ConsumeUntil('`').Replace("\\`", "`");
		try
		{
			var json = JsonNode.Parse(consumed);
			return JmesToken.LiteralToken(json);
		}
		catch (JsonException e)
		{
			throw new TokenizationException(
				$"Failed to parse JSON inside of the literal expression at {start}",
				e
			);
		}
	}

	/// <summary>
	/// Consumes the left bracket from the input stream, returning tokens depending on the peeked character.
	/// </summary>
	/// <returns>A <see cref="IToken"/> representing the consumed bracket and other related tokens.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private JmesToken ConsumeBracket()
	{
		switch (_enumerator.Peek())
		{
			case '?':
				MoveNext();
				return JmesToken.Make(JmesTokenType.Filter);
			case ']':
				MoveNext();
				return JmesToken.Make(JmesTokenType.Flatten);
			default:
				return JmesToken.Make(JmesTokenType.LBracket);
		}
	}

	/// <summary>
	/// Consumes a number from the input stream.
	/// </summary>
	/// <param name="negative">whether the number is negative</param>
	/// <returns>A <see cref="NumberToken"/> representing the consumed number.</returns>
	/// <exception cref="TokenizationException">the input is not a valid number</exception>
	private JmesToken ConsumeNumber(bool negative = false)
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
			return JmesToken.NumberToken(negative ? -value : value);
		}
		catch (FormatException ex)
		{
			throw new TokenizationException(
				$"Invalid number format at index {startingIndex}: {consumed}",
				ex
			);
		}
	}
}
