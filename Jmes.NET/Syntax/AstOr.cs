namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a logical "or" operation between two expressions.
/// </summary>
public sealed class AstOr : AstNode
{
	public required AstNode Left { get; init; }
	public required AstNode Right { get; init; }
}
