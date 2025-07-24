namespace Jmes.NET;

public interface IToken
{
	public int LeftBindingPower => 0;
}

public sealed class PipeToken : IToken { }
public sealed class OrToken : IToken { }
public sealed class AndToken : IToken { }
public sealed class NotToken : IToken { }
public sealed class LParenToken : IToken { }
public sealed class RParenToken : IToken { }
public sealed class LBracketToken : IToken { }
public sealed class RBracketToken : IToken { }
public sealed class ColonToken : IToken { }
public sealed class IdentifierToken : IToken { }
public sealed class QuotedIdentifierToken : IToken { }
public sealed class DotToken : IToken { }
public sealed class EqToken : IToken { }
public sealed class NeqToken : IToken { }
public sealed class GreaterThanToken : IToken { }
public sealed class LessThanToken : IToken { }
public sealed class StarToken : IToken { }
public sealed class PlusToken : IToken { }
public sealed class MinusToken : IToken { }
public sealed class FilterToken : IToken { }
public sealed class FlattenToken : IToken { }

