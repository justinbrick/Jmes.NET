using System.Text.RegularExpressions;
using Jmes.NET.Syntax;

namespace Jmes.NET;

/// <summary>
///	 A parser for JMESPath tokens.
/// </summary>
/// <param name="queue">a queue of the tokens to parse</param>
public sealed class Parser(Queue<(long, JmesToken)> queue)
{
	/// <summary>
	/// The max binding power of a token before projection ends.
	/// </summary>
	const int ProjectionStop = 5;

	private long _index;

	private (long, JmesToken) MoveNextWithPosition()
	{
		if (queue.TryDequeue(out var token))
		{
			_index = token.Item1;
			return token;
		}
		else
		{
			return (_index, JmesToken.Make(JmesTokenType.Eof));
		}
	}

	private JmesToken MoveNext()
	{
		return MoveNextWithPosition().Item2;
	}

	private JmesToken Peek(int offset = 0)
	{
		if (offset == 0)
		{
			return queue.TryPeek(out var token) ? token.Item2 : JmesToken.Make(JmesTokenType.Eof);
		}

		try
		{
			return queue.ElementAt(offset).Item2;
		}
		catch (ArgumentOutOfRangeException)
		{
			return JmesToken.Make(JmesTokenType.Eof);
		}
	}

	private AstNode Evaluate(int rbp)
	{
		var left = NullDenotation();

		while (rbp < Peek().LeftBindingPower)
		{
			left = LeftDenotation(left);
		}

		return left;
	}

	private AstNode NullDenotation()
	{
		var (position, token) = MoveNextWithPosition();
		switch (token.Type)
		{
			case JmesTokenType.At:
				return new AstIdentity { Offset = position };
			case JmesTokenType.Identifier:
				return new AstField { Offset = position, FieldName = (string)token.Value! };
			case JmesTokenType.QuotedIdentifier:
				if (Peek().Type == JmesTokenType.LParen)
				{
					throw new ParsingException(
						$"Quoted strings cannot be used as function identifiers, position {position}"
					);
				}
				return new AstField { Offset = position, FieldName = (string)token.Value! };
			case JmesTokenType.Star:
				return ParseWildcardProjection(new AstIdentity { Offset = position });
			case JmesTokenType.Literal:
				return new AstLiteral { Offset = position, Value = token.Value };
			case JmesTokenType.LBracket:
				switch (Peek().Type)
				{
					case JmesTokenType.Number:
					case JmesTokenType.Colon:
						return ParseIndex();
					case JmesTokenType.Star when Peek(1).Type == JmesTokenType.RBracket:
						MoveNext();
						return ParseWildcardIndex(new AstIdentity { Offset = position });
					default:
						return ParseMultiSelectList();
				}
			case JmesTokenType.Flatten:
			case JmesTokenType.LBrace:
			case JmesTokenType.Ampersand:
			case JmesTokenType.Not:
			case JmesTokenType.Filter:
			case JmesTokenType.LParen:
				throw new NotImplementedException();
			default:
				throw new ParsingException($"Unexpected token {token.Type} at position {position}");
		}
	}

	private AstNode LeftDenotation(AstNode left)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Parses a <see href="https://jmespath.org/specification.html#wildcard-expressions">projection expression</see>, defined as lhs into rhs
	/// </summary>
	/// <remarks>
	/// The RHS of a projection can one of
	/// <list type="bullet">
	///		<item>a dot (.) followed by an identifier</item>
	///		<item>a bracket ([) followed by an index or slice</item>
	///		<item>a filter ([?) followed by an expression</item>
	/// </list>
	/// </remarks>
	/// <param name="lbp">the binding power of the left-hand side expression</param>
	/// <returns>a node representing the right-hand side of a projection expression</returns>
	/// <exception cref="ParsingException"></exception>
	private AstNode ParseProjection(int lbp)
	{
		switch (Peek().Type)
		{
			case JmesTokenType.Dot:
				MoveNext();
				throw new NotImplementedException("Dot projection not implemented");
			case JmesTokenType.LBracket:
			case JmesTokenType.Filter:
				return Evaluate(lbp);
			case JmesTokenType type when JmesToken.GetLeftBindingPower(type) < ProjectionStop:
				return new AstIdentity { Offset = _index };
			default:
				throw new ParsingException($"Expected '.', '[', or '[?', found {Peek().Type}");
		}
	}

	private AstNode ParseIndex()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// When given a wildcard (*), parses a projection expression.
	/// </summary>
	/// <param name="lhs">the left-hand side expression</param>
	/// <returns>a projection expression</returns>
	private AstProjection ParseWildcardProjection(AstNode lhs)
	{
		var rhs = ParseProjection(JmesToken.GetLeftBindingPower(JmesTokenType.Star));
		return new AstProjection
		{
			Offset = _index,
			Left = new AstObjectValues { Offset = _index, Node = lhs },
			Right = rhs,
		};
	}

	private AstNode ParseWildcardIndex(AstNode lhs)
	{
		throw new NotImplementedException();
	}

	private AstNode ParseMultiSelectList()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Equivalent to jmes-rs parse_kvp
	/// </summary>
	private AstHashMap ParseHashMap()
	{
		throw new NotImplementedException();
	}

	private AstProjection ParseFilter(AstNode lhs)
	{
		throw new NotImplementedException();
	}

	private AstFlatten ParseFlatten(AstNode lhs)
	{
		throw new NotImplementedException();
	}

	private AstComparison ParseComparison(AstComparisonOperator operand, AstNode lhs)
	{
		throw new NotImplementedException();
	}

	private AstNode ParseDot(int lbp)
	{
		throw new NotImplementedException();
	}

	private AstNode ParseList(JmesToken sentinel)
	{
		throw new NotImplementedException();
	}
}
