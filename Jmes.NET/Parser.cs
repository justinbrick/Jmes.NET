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

	private JmesToken Peek()
	{
		return queue.TryPeek(out var token) ? token.Item2 : JmesToken.Make(JmesTokenType.Eof);
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
		return token.Type switch
		{
			_ => throw new ParsingException($"Unexpected token {token} at position {position}"),
		};
	}

	private AstNode LeftDenotation(AstNode left)
	{
		throw new NotImplementedException();
	}
}
