using Jmes.NET.Tokenization;

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
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Dot)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize(".")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Star)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("*")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.At)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("@")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[
					(0, JmesToken.Make(JmesTokenType.RBracket)),
					(1, JmesToken.Make(JmesTokenType.Eof)),
				]
			),
			Tokenizer.Tokenize("]")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.LBrace)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("{")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.RBrace)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("}")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.LParen)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("(")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.RParen)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize(")")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Comma)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize(",")
		);
	}

	[Fact]
	public void HandlesLeftBrackets()
	{
		// expect flatten
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Flatten)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("[]")
		);

		// expect filter
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Filter)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("[?")
		);

		// expect lbracket
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[
					(0, JmesToken.Make(JmesTokenType.LBracket)),
					(1, JmesToken.Make(JmesTokenType.Eof)),
				]
			),
			Tokenizer.Tokenize("[")
		);
	}

	[Fact]
	public void HandlesPipeAndOr()
	{
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Pipe)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("|")
		);

		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Or)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("||")
		);
	}

	[Fact]
	public void HandlesAmpersand()
	{
		// &
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[
					(0, JmesToken.Make(JmesTokenType.Ampersand)),
					(1, JmesToken.Make(JmesTokenType.Eof)),
				]
			),
			Tokenizer.Tokenize("&")
		);

		// && (and)
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.And)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("&&")
		);
	}

	[Fact]
	public void HandlesComparisonOperators()
	{
		// gt
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Gt)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize(">")
		);

		// gte
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Gte)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize(">=")
		);

		// lt
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Lt)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("<")
		);

		// lte
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Lte)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("<=")
		);
	}

	[Fact]
	public void HandlesEqualityOperators()
	{
		// eq
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Eq)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("==")
		);

		// neq
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Neq)), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("!=")
		);

		// not
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Make(JmesTokenType.Not)), (1, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("!")
		);
	}

	[Fact]
	public void HandlesIdentifiers()
	{
		// jmes_path
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Identifier("jmes_path")), (9, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("jmes_path")
		);

		// jmes
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Identifier("jmes")), (4, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("jmes")
		);

		// _jmes
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.Identifier("_jmes")), (5, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("_jmes")
		);
	}

	[Fact]
	public void HandlesQuotedIdentifiers()
	{
		// "jmes"
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.QuotedIdentifier("jmes")), (6, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("\"jmes\"")
		);

		// ""
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.QuotedIdentifier("")), (2, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("\"\"")
		);

		// "a_b"
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.QuotedIdentifier("a_b")), (5, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("\"a_b\"")
		);

		// "a\nb"
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.QuotedIdentifier("a\nb")), (6, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("\"a\\nb\"")
		);

		// "a\\nb"
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[(0, JmesToken.QuotedIdentifier("a\\nb")), (7, JmesToken.Make(JmesTokenType.Eof))]
			),
			Tokenizer.Tokenize("\"a\\\\nb\"")
		);
	}

	[Fact]
	public void InvalidEqualFails()
	{
		_ = Assert.Throws<TokenizationException>(static () => Tokenizer.Tokenize("="));
	}

	[Fact]
	public void IgnoresWhitespace()
	{
		Assert.Equal(
			new Queue<(long, JmesToken)>(
				[
					(5, JmesToken.Make(JmesTokenType.Dot)),
					(7, JmesToken.Make(JmesTokenType.LParen)),
					(8, JmesToken.Make(JmesTokenType.Eof)),
				]
			),
			Tokenizer.Tokenize(" \t\n\r\t. (")
		);
	}

	[Fact]
	public void InvalidCharacterFails()
	{
		_ = Assert.Throws<TokenizationException>(static () => Tokenizer.Tokenize("#"));
		_ = Assert.Throws<TokenizationException>(static () => Tokenizer.Tokenize("~"));
	}

	[Fact]
	public void UnterminatedCharacterFails()
	{
		// " and `
		_ = Assert.Throws<TokenizationException>(static () => Tokenizer.Tokenize("\"bar"));
		_ = Assert.Throws<TokenizationException>(static () => Tokenizer.Tokenize("`{}"));
	}
}
