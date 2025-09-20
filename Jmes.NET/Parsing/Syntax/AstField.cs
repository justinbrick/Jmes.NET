namespace Jmes.NET.Parsing.Syntax;

/// <summary>
/// Represents a field in a map, inside the abstract syntax tree.
/// </summary>
public sealed class AstField : AstNode
{
	public required string FieldName { get; init; }
}
