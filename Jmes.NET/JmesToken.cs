using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Security.Principal;

namespace Jmes.NET;

/// <summary>
/// Token types used by the tokenizer and parser.
/// </summary>
public enum JmesTokenType
{
	QuotedIdentifier,
	Identifier,
	Number,
	Literal,
	Pipe,
	Or,
	And,
	Not,
	LParen,
	RParen,
	LBracket,
	RBracket,
	LBrace,
	RBrace,
	Colon,
	Dot,
	Eq,
	Neq,
	Gt,
	Lt,
	Gte,
	Lte,
	Star,
	Filter,
	Flatten,
	Ampersand,
	Comma,
	At,
	Eof,
}

/// <summary>
/// Single value type representing any token. Uses a record struct so equality is value-based.
/// Payloads are stored in the optional fields depending on token type.
/// </summary>
public readonly record struct JmesToken(JmesTokenType Type, object? Value = null)
{
	public static int GetLeftBindingPower(JmesTokenType type) =>
		type switch
		{
			JmesTokenType.Pipe => 1,
			JmesTokenType.Or => 2,
			JmesTokenType.And => 3,
			JmesTokenType.Eq => 4,
			JmesTokenType.Gt => 4,
			JmesTokenType.Lt => 4,
			JmesTokenType.Gte => 4,
			JmesTokenType.Lte => 4,
			JmesTokenType.Neq => 4,
			JmesTokenType.Flatten => 5,
			JmesTokenType.Star => 6,
			JmesTokenType.Filter => 7,
			JmesTokenType.Dot => 8,
			JmesTokenType.Not => 9,
			JmesTokenType.LBrace => 10,
			JmesTokenType.LBracket => 11,
			JmesTokenType.LParen => 12,
			_ => 0,
		};

	public int LeftBindingPower => GetLeftBindingPower(Type);

	public static JmesToken Make(JmesTokenType type) => new(type);

	public static JmesToken QuotedIdentifier(string value) =>
		new(JmesTokenType.QuotedIdentifier, value);

	public static JmesToken Identifier(string value) => new(JmesTokenType.Identifier, value);

	public static JmesToken Number(int value) => new(JmesTokenType.Number, value);

	public static JmesToken Literal(object? value) => new(JmesTokenType.Literal, value);
}
