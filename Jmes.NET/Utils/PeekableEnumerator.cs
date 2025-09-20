using System.Collections;

namespace Jmes.NET.Utils;

/// <summary>
/// An enumerator implementation that allows peeking at the next element without advancing the enumeration.
/// </summary>
/// <typeparam name="E"></typeparam>
/// <typeparam name="T"></typeparam>
/// <param name="enumerator"></param>
internal sealed class PeekableEnumerator<E, T>(E enumerator) : IEnumerator<T>
	where E : IEnumerator<T>
	where T : struct
{
	private T? _next = enumerator.MoveNext() ? enumerator.Current : null;
	private T? _current;

	public T Current
	{
		get
		{
			return _current
				?? throw new InvalidOperationException(
					"Enumeration has not started. Call MoveNext."
				);
		}
		private set => _current = value;
	}

	object IEnumerator.Current => Current!;

	public void Dispose() => enumerator.Dispose();

	public bool MoveNext()
	{
		var moved = _next.HasValue;
		if (_next.HasValue)
		{
			_current = _next.Value;
		}

		_next = enumerator.MoveNext() ? enumerator.Current : null;

		return moved;
	}

	public void Reset()
	{
		enumerator.Reset();
		_next = enumerator.MoveNext() ? enumerator.Current : null;
	}

	public T? Peek()
	{
		return _next;
	}
}
