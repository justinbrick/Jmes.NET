namespace Jmes.NET.Parsing.Syntax;

/// <summary>
/// Represents a reference to an expression in the abstract syntax tree.
/// </summary>
public sealed class AstExpressionRef : AstNode
{
	public required AstNode Expression { get; init; }
}
