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
}
