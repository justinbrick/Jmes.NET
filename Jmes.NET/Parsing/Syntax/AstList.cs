namespace Jmes.NET.Parsing.Syntax;

public sealed class AstList : AstNode
{
	public required List<AstNode> Items { get; init; }
}
