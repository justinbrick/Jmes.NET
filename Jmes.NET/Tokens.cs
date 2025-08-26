namespace Jmes.NET;

public interface IToken
{
	public int LeftBindingPower => 0;
}

public sealed class QuotedIdentifierToken(string value) : IToken
{
	public string Value = value;
}

public sealed class IdentifierToken(string value) : IToken
{
	public string Value = value;
}

public sealed class NumberToken(int value) : IToken
{
	public int Value = value;
}

public sealed class LiteralToken : IToken
{
	public required object Value { get; init; }
}

public sealed class PipeToken : IToken { }

public sealed class OrToken : IToken { }

public sealed class AndToken : IToken { }

public sealed class NotToken : IToken { }

public sealed class LParenToken : IToken { }

public sealed class RParenToken : IToken { }

public sealed class LBracketToken : IToken { }

public sealed class RBracketToken : IToken { }

public sealed class LBraceToken : IToken { }

public sealed class RBraceToken : IToken { }

public sealed class ColonToken : IToken { }

public sealed class DotToken : IToken { }

public sealed class EqToken : IToken { }

public sealed class NeqToken : IToken { }

public sealed class GtToken : IToken { }

public sealed class LtToken : IToken { }

public sealed class GteToken : IToken { }

public sealed class LteToken : IToken { }

public sealed class StarToken : IToken { }

public sealed class FilterToken : IToken { }

public sealed class FlattenToken : IToken { }

public sealed class AmpersandToken : IToken { }

public sealed class CommaToken : IToken { }

public sealed class AtToken : IToken { }

public sealed class EofToken : IToken { }
