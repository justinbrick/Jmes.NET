namespace Jmes.NET.Tests;

public class TokenizerTests
{
	/// <summary>
	/// Tests for non-complex tokens, that don't require any custom logic.
	/// </summary>
	[Fact]
	public void HandlesBasicTokens()
	{
		// . * @ ] { } ( ) ,
		Assert.Equal(
			Tokenizer.Tokenize("."),
			[(0, JmesToken.Make(JmesTokenType.Dot)), (1, JmesToken.Make(JmesTokenType.Eof))]
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Star)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("*")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.At)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("@")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.RBracket)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("]")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.LBrace)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("{")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.RBrace)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("}")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.LParen)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("(")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.RParen)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize(")")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Comma)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize(",")
		);
	}

	[Fact]
	public void HandlesLeftBrackets()
	{
		// expect flatten
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Flatten)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("[]")
		);

		// expect filter
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Filter)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("[?")
		);

		// expect lbracket
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.LBracket)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("[")
		);
	}

	[Fact]
	public void HandlesPipeAndOr()
	{
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Pipe)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("|")
		);

		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Or)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("||")
		);
	}

	[Fact]
	public void HandlesAmpersand()
	{
		// &
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Ampersand)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("&")
		);

		// && (and)
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.And)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("&&")
		);
	}

	[Fact]
	public void HandlesComparisonOperators()
	{
		// gt
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Gt)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize(">")
		);

		// gte
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Gte)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize(">=")
		);

		// lt
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Lt)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("<")
		);

		// lte
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Lte)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("<=")
		);
	}

	[Fact]
	public void HandlesEqualityOperators()
	{
		// eq
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Eq)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("==")
		);

		// neq
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Neq)), (2, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("!=")
		);

		// not
		Assert.Equal(
			[(0, JmesToken.Make(JmesTokenType.Not)), (1, JmesToken.Make(JmesTokenType.Eof))],
			Tokenizer.Tokenize("!")
		);
	}

	[Fact]
	public void InvalidEqualFails()
	{
		Assert.Throws<TokenizationException>(() => Tokenizer.Tokenize("="));
	}

	[Fact]
	public void IgnoresWhitespace()
	{
		Assert.Equal(
			[
				(5, JmesToken.Make(JmesTokenType.Dot)),
				(7, JmesToken.Make(JmesTokenType.LParen)),
				(8, JmesToken.Make(JmesTokenType.Eof)),
			],
			Tokenizer.Tokenize(" \t\n\r\t. (")
		);
	}

	[Fact]
	public void InvalidCharacterFails()
	{
		Assert.Throws<TokenizationException>(() => Tokenizer.Tokenize("#"));
		Assert.Throws<TokenizationException>(() => Tokenizer.Tokenize("~"));
	}
}
