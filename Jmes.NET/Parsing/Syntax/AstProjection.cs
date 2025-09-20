namespace Jmes.NET.Parsing.Syntax;

public sealed class AstProjection : AstNode
{
	public required AstNode Left { get; init; }
	public required AstNode Right { get; init; }
}
