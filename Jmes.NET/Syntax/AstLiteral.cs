namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a literal value in the abstract syntax tree.
/// </summary>
public sealed class AstLiteral : AstNode
{
	/// <summary>
	/// Represents a literal value in the abstract syntax tree.
	/// </summary>
	public required object? Value { get; init; }
}
