namespace Jmes.NET.Parsing.Syntax;

/// <summary>
/// Represents a sub-expression in the abstract syntax tree.
/// </summary>
public sealed class AstSubExpression : AstNode
{
	public required AstNode Left { get; init; }
	public required AstNode Right { get; init; }
}
