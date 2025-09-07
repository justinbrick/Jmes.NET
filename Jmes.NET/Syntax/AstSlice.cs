namespace Jmes.NET.Syntax;

public sealed class AstSlice : AstNode
{
	public int? Start { get; init; }
	public int? End { get; init; }
	public int Step { get; init; }
}
