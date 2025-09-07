namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a flatten operation in the abstract syntax tree.
/// </summary>
public sealed class AstFlatten : AstNode
{
	public required AstNode Node { get; init; }
}
