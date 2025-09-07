namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a conditional expression in the abstract syntax tree.
/// </summary>
public sealed class AstCondition : AstNode
{
	/// <summary>
	/// The condition to evaluate.
	/// </summary>
	public required AstNode Predicate { get; init; }

	/// <summary>
	/// The result to return if the condition is true.
	/// </summary>
	public required AstNode Then { get; init; }
}
