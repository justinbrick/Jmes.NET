namespace Jmes.NET.Syntax;

public sealed class AstList : AstNode
{
	public required List<AstNode> Items { get; init; }
}
