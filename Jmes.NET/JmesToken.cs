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
	public int LeftBindingPower => 0;

	public static JmesToken Make(JmesTokenType type) => new(type);

	public static JmesToken QuotedIdentifier(string value) =>
		new(JmesTokenType.QuotedIdentifier, value);

	public static JmesToken Identifier(string value) => new(JmesTokenType.Identifier, value);

	public static JmesToken NumberToken(int value) => new(JmesTokenType.Number, value);

	public static JmesToken LiteralToken(object? value) => new(JmesTokenType.Literal, value);
}
