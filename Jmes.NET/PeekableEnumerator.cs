using System.Collections;

namespace Jmes.NET;

internal sealed class PeekableEnumerator<T> : IEnumerator<T>
	where T : notnull
{
	private readonly IEnumerator<T> _enumerator;
	private T? _next;

	public T Current { get; private set; }

	object IEnumerator.Current => Current;

	public PeekableEnumerator(IEnumerator<T> enumerator)
	{
		_enumerator = enumerator;
		Current = enumerator.Current;
		_next = enumerator.MoveNext() ? enumerator.Current : default;
	}

	public void Dispose()
	{
		_enumerator.Dispose();
	}

	public bool MoveNext()
	{
		bool moved = false;

		if (_next is not null)
		{
			Current = _next;
			moved = true;
		}

		_next = _enumerator.MoveNext() ? _enumerator.Current : default;

		return moved;
	}

	public void Reset()
	{
		_enumerator.Reset();
		Current = _enumerator.Current;
		_next = _enumerator.MoveNext() ? _enumerator.Current : default;
	}

	public T? Peek()
	{
		return _next;
	}
}
