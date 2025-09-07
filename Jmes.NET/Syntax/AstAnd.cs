namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a logical "and" operation between two expressions.
/// </summary>
public sealed class AstAnd : AstNode
{
	public required AstNode Left { get; init; }
	public required AstNode Right { get; init; }
}
