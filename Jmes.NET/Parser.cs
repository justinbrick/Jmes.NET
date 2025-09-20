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
				return ParseFlatten(new AstIdentity { Offset = position });
			case JmesTokenType.LBrace:
				return ParseHashMap();
			case JmesTokenType.Ampersand:
				return new AstExpressionRef
				{
					Offset = position,
					Expression = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.Ampersand)),
				};
			case JmesTokenType.Not:
				return new AstNot
				{
					Offset = position,
					Operand = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.Not)),
				};
			case JmesTokenType.Filter:
				return ParseFilter(new AstIdentity { Offset = position });
			case JmesTokenType.LParen:
				var expr = Evaluate(0);
				var next = MoveNext();
				if (next.Type != JmesTokenType.RParen)
				{
					throw new ParsingException(
						$"Expected ')', found {next.Type} at position {_index}"
					);
				}
				return expr;
			default:
				throw new ParsingException($"Unexpected token {token.Type} at position {position}");
		}
	}

	private AstNode LeftDenotation(AstNode left)
	{
		var (position, token) = MoveNextWithPosition();
		switch (token.Type)
		{
			case JmesTokenType.Dot:
				if (Peek().Type == JmesTokenType.Star)
				{
					MoveNext();
					return ParseWildcardProjection(left);
				}

				return new AstSubExpression
				{
					Offset = position,
					Left = left,
					Right = ParseDot(JmesToken.GetLeftBindingPower(JmesTokenType.Dot)),
				};
			case JmesTokenType.LBracket:
				switch (Peek().Type)
				{
					case JmesTokenType.Number:
					case JmesTokenType.Colon:
						return new AstSubExpression
						{
							Offset = position,
							Left = left,
							Right = ParseIndex(),
						};
					case JmesTokenType.Star:
						MoveNext();
						return ParseWildcardIndex(left);
					default:
						throw new ParsingException(
							$"Unexpected token {Peek().Type} after '[' at position {_index}. Expected number, colon, or '*'"
						);
				}
			case JmesTokenType.Or:
				return new AstOr
				{
					Offset = position,
					Left = left,
					Right = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.Or)),
				};
			case JmesTokenType.And:
				return new AstAnd
				{
					Offset = position,
					Left = left,
					Right = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.And)),
				};
			case JmesTokenType.Pipe:
				return new AstSubExpression
				{
					Offset = position,
					Left = left,
					Right = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.Pipe)),
				};
			case JmesTokenType.LParen:
				if (left is not AstField field)
				{
					throw new ParsingException(
						$"Expected function name before '(', found {left.GetType().Name} at position {position}"
					);
				}

				return new AstFunction
				{
					Offset = position,
					Name = field.FieldName,
					Arguments = ParseList(JmesTokenType.RParen),
				};
			case JmesTokenType.Flatten:
				return ParseFlatten(left);
			case JmesTokenType.Filter:
				return ParseFilter(left);
			case JmesTokenType.Eq:
				return ParseComparison(AstComparisonOperator.Eq, left);
			case JmesTokenType.Neq:
				return ParseComparison(AstComparisonOperator.Neq, left);
			case JmesTokenType.Gt:
				return ParseComparison(AstComparisonOperator.Gt, left);
			case JmesTokenType.Gte:
				return ParseComparison(AstComparisonOperator.Gte, left);
			case JmesTokenType.Lt:
				return ParseComparison(AstComparisonOperator.Lt, left);
			case JmesTokenType.Lte:
				return ParseComparison(AstComparisonOperator.Lte, left);
			default:
				throw new ParsingException($"Unexpected token {token.Type} at position {position}");
		}
	}

	/// <summary>
	/// Parses a <see href="https://jmespath.org/specification.html#wildcard-expressions">projection expression</see>, defined as lhs into rhs
	/// </summary>
	/// <remarks>
	/// The RHS of a projection can be one of
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
				return ParseDot(lbp);
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
		int?[] indices = [null, null, null];
		var idx = 0;
		var notDone = true;
		while (notDone)
		{
			var next = MoveNext();
			switch (next.Type)
			{
				case JmesTokenType.Number:
					indices[idx] = (int)next.Value!;
					break;
				case JmesTokenType.RBracket:
					notDone = false;
					break;
				case JmesTokenType.Colon:
					if (idx >= 2)
					{
						throw new ParsingException(
							$"Too many colons in index/slice at position {_index}"
						);
					}
					idx++;
					break;
				default:
					throw new ParsingException(
						$"Unexpected token {next.Type} in index/slice at position {_index}. Expected number, colon, or ']'"
					);
			}
		}

		// Simple indexing
		if (idx == 0)
		{
#if DEBUG
			// Given how we parse, this should never happen.
			// If it does, it's a bug in the parser.
			if (!indices[0].HasValue)
			{
				throw new InvalidOperationException("Expected index to have a value");
			}
#endif
			return new AstIndex { Offset = _index, Index = indices[0]!.Value };
		}

		// Slicing
		return new AstProjection
		{
			Offset = _index,
			Left = new AstSlice
			{
				Offset = _index,
				Start = indices[0],
				End = indices[1],
				Step = indices[2] ?? 1,
			},
			Right = ParseProjection(JmesToken.GetLeftBindingPower(JmesTokenType.Star)),
		};
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

	private AstProjection ParseWildcardIndex(AstNode lhs)
	{
		var next = MoveNext();
		return next.Type != JmesTokenType.RBracket
			? throw new ParsingException($"Expected ']', found {next.Type}")
			: new AstProjection
			{
				Offset = _index,
				Left = lhs,
				Right = ParseProjection(JmesToken.GetLeftBindingPower(JmesTokenType.Star)),
			};
	}

	/// <summary>
	/// Parses a multi-select list, e.g. [a, b, c]
	/// </summary>
	/// <returns>the parsed AST list node</returns>
	private AstList ParseMultiSelectList()
	{
		return new AstList { Offset = _index, Items = ParseList(JmesTokenType.RBracket) };
	}

	/// <summary>
	/// Equivalent to jmes-rs parse_kvp
	/// </summary>
	private AstHashMap ParseHashMap()
	{
		var values = new List<(string Key, AstNode Value)>();
		var notDone = true;
		while (notDone)
		{
			var expectedKey = MoveNext();
			switch (expectedKey.Type)
			{
				case JmesTokenType.QuotedIdentifier:
				case JmesTokenType.Identifier:
					if (MoveNext().Type != JmesTokenType.Colon)
					{
						throw new ParsingException(
							$"Expected ':' after key in object at position {_index}"
						);
					}

					values.Add(((string)expectedKey.Value!, Evaluate(0)));
					break;
				default:
					throw new ParsingException(
						$"Expected identifier or quoted identifier for key, found {expectedKey.Type} at position {_index}"
					);
			}

			switch (MoveNext().Type)
			{
				case JmesTokenType.Comma:
					continue;
				case JmesTokenType.RBrace:
					notDone = false;
					break;
				default:
					throw new ParsingException($"Expected ',' or '}}' in object, found {_index}");
			}
		}

		return new AstHashMap { Offset = _index, Entries = values };
	}

	private AstProjection ParseFilter(AstNode lhs)
	{
		var conditionLhs = Evaluate(0);
		return Peek().Type != JmesTokenType.RBracket
			? throw new ParsingException(
				$"Expected ']' after filter expression, found {Peek().Type} at position {_index}"
			)
			: new AstProjection
			{
				Offset = _index,
				Left = lhs,
				Right = new AstCondition
				{
					Offset = _index,
					Predicate = conditionLhs,
					Then = ParseProjection(JmesToken.GetLeftBindingPower(JmesTokenType.Filter)),
				},
			};
	}

	private AstProjection ParseFlatten(AstNode lhs)
	{
		var right = ParseProjection(JmesToken.GetLeftBindingPower(JmesTokenType.Flatten));
		return new AstProjection
		{
			Offset = _index,
			Left = new AstFlatten { Offset = _index, Node = lhs },
			Right = right,
		};
	}

	private AstComparison ParseComparison(AstComparisonOperator op, AstNode lhs)
	{
		// all comparison operators have the same binding power
		var right = Evaluate(JmesToken.GetLeftBindingPower(JmesTokenType.Eq));
		return new AstComparison
		{
			Offset = _index,
			Left = lhs,
			Operator = op,
			Right = right,
		};
	}

	private AstNode ParseDot(int lbp)
	{
		switch (Peek().Type)
		{
			case JmesTokenType.LBracket:
				MoveNext();
				return ParseMultiSelectList();
			case JmesTokenType.Identifier:
			case JmesTokenType.QuotedIdentifier:
			case JmesTokenType.Star:
			case JmesTokenType.LBrace:
			case JmesTokenType.Ampersand:
				return Evaluate(lbp);
			default:
				throw new ParsingException(
					$"Expected identifier, quoted identifier, '*', '{{', or '&' after '.', found {Peek().Type} at position {_index}"
				);
		}
	}

	/// <summary>
	/// Parses a comma-separated list of expressions until the sentinel token is reached.
	/// </summary>
	/// <param name="sentinel">the token type to stop parsing at</param>
	/// <returns>a list of parsed AST nodes</returns>
	/// <exception cref="ParsingException">a trailing comma was encountered before the sentinel token is encountered</exception>
	private List<AstNode> ParseList(JmesTokenType sentinel)
	{
		var items = new List<AstNode>();
		while (Peek().Type != sentinel)
		{
			items.Add(Evaluate(0));
			if (Peek().Type == JmesTokenType.Comma)
			{
				MoveNext();
				if (Peek().Type == sentinel)
				{
					throw new ParsingException(
						$"Trailing comma before {sentinel} at position {_index}"
					);
				}
			}
		}
		// Clear the sentinel
		MoveNext();
		return items;
	}
}
