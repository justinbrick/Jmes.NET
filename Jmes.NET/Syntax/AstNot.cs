namespace Jmes.NET.Syntax;

public sealed class AstNot : AstNode
{
	/// <summary>
	/// The node that the "not" operator is applied to.
	/// </summary>
	public required AstNode Operand { get; init; }
}
