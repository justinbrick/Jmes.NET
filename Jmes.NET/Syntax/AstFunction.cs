namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a function call in the abstract syntax tree.
/// </summary>
public sealed class AstFunction : AstNode
{
	public required string Name { get; init; }
	public required List<AstNode> Arguments { get; init; }
}
