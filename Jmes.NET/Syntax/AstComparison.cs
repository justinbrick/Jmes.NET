namespace Jmes.NET.Syntax;

/// <summary>
/// Represents a comparison expression in the abstract syntax tree.
/// </summary>
public sealed class AstComparison : AstNode
{
	/// <summary>
	/// The left-hand side of the comparison.
	/// </summary>
	public required AstNode Left { get; init; }

	/// <summary>
	/// The right-hand side of the comparison.
	/// </summary>
	public required AstNode Right { get; init; }

	/// <summary>
	/// The operator to use for the comparison.
	/// </summary>
	public required AstComparisonOperator Operator { get; init; }

	/// <summary>
	/// Gets the comparison operator for the given token.
	/// </summary>
	/// <param name="token">the token to get the operator for</param>
	/// <returns>the comparison operator that corresponds to the token</returns>
	/// <exception cref="ArgumentOutOfRangeException">the <paramref name="token"/> is not a valid comparison operator token</exception>
	public static AstComparisonOperator GetOperator(JmesToken token)
	{
		return token.Type switch
		{
			JmesTokenType.Eq => AstComparisonOperator.Eq,
			JmesTokenType.Neq => AstComparisonOperator.Neq,
			JmesTokenType.Lt => AstComparisonOperator.Lt,
			JmesTokenType.Lte => AstComparisonOperator.Lte,
			JmesTokenType.Gt => AstComparisonOperator.Gt,
			JmesTokenType.Gte => AstComparisonOperator.Gte,
			_ => throw new ArgumentOutOfRangeException(
				nameof(token),
				token,
				"Expected comparison operator token."
			),
		};
	}
}

/// <summary>
/// Represents a comparison operator in the abstract syntax tree.
/// </summary>
public enum AstComparisonOperator
{
	Eq,
	Neq,
	Lt,
	Lte,
	Gt,
	Gte,
}
