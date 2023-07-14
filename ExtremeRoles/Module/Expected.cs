using System;
using System.Collections.Generic;


#nullable enable

namespace ExtremeRoles.Module;

public static class ErrorCodeMaker
{
	public class ErrorCode<E>
		where E :
			struct, IComparable, IConvertible,
			IComparable<E>, IEquatable<E>
	{
		private readonly E? code;

		// 基本的にこいつを呼ばない
		public ErrorCode()
		{
			code = default(E);
		}

		public ErrorCode(E code)
		{
			this.code = code;
		}
		public string GetMessage()
		{
			if (this.code == null) { return ""; }
			string? message = this.code.ToString();

			if (string.IsNullOrEmpty(message)) { return ""; }

			return message;
		}
	}

	public static Unexpected<ErrorCode<E>> MakeErrorCode<E>(E code)
		where E :
			struct, IComparable, IConvertible,
			IComparable<E>, IEquatable<E>
		=> new Unexpected<ErrorCode<E>>(new ErrorCode<E>(code));
}

public sealed class Unexpected<E>
	where E : new()
{
	private readonly E e;

	public Unexpected()
	{
		this.e = new E();
	}
	public Unexpected(E e)
	{
		this.e = e;
	}

	public static implicit operator E(Unexpected<E> exp)
	{
		return exp.e;
	}

	public static implicit operator Unexpected<E>(E e)
	{
		return new Unexpected<E>(e);
	}
}


public sealed class Expected<T, E> : IEquatable<Expected<T, E>>
	where E : new()
{
	public T Value
	{
		get
		{
			if (this.value == null)
			{
				throw new InvalidOperationException();
			}
			return this.value;
		}
	}
	public Unexpected<E> Error { get; init; }

	private T? value;

	public Expected(T value)
	{
		this.value = value;
		Error = new Unexpected<E>();
	}

	public Expected(Unexpected<E> error)
	{
		this.value = default(T);
		Error = error;
	}

	public bool HasValue()
		=> this.value != null;

	public Expected<U, E> AndThen<U>(Func<T, Expected<U, E>> lamda)
		=> this.value != null ?
		lamda.Invoke(this.value) :
		new Expected<U, E>(this.Error);

	public Expected<T, X> OrElse<X>(Func<Unexpected<E>, Expected<T, X>> lamda)
		where X : new()
		=> this.value != null ?
		new Expected<T, X>(this.Value) :
		lamda.Invoke(this.Error);

	public Expected<X, E> Transform<X>(Func<T, X> lamda)
		=> this.value != null ?
		new Expected<X, E>(lamda.Invoke(this.Value)) :
		new Expected<X, E>(this.Error);

	public Expected<T, X> TransformError<X>(Func<E, X> lamda)
		where X : new()
		=> this.value != null ?
		new Expected<T, X>(this.value) :
		new Expected<T, X>(lamda.Invoke(this.Error));

	public bool Equals(Expected<T, E>? other)
	{
		if (other is null) { return false; }

		bool rightHasValue = this.HasValue();
		bool leftHasValue = other.HasValue();

		if (rightHasValue && leftHasValue)
		{
			return EqualityComparer<T>.Default.Equals(other.Value, this.Value);
		}
		else if (rightHasValue || leftHasValue)
		{
			return false;
		}
		else
		{
			return EqualityComparer<E>.Default.Equals(other.Error, this.Error);
		}
	}

	public static bool operator ==(Expected<T, E> left, Expected<T, E>? right)
		=> left.Equals(right);

	public static bool operator !=(Expected<T, E> left, Expected<T, E>? right)
		=> !left.Equals(right);

	public static implicit operator Expected<T, E>(T v)
	{
		return new Expected<T, E>(v);
	}

	public static implicit operator Expected<T, E>(E e)
	{
		return new Expected<T, E>(e);
	}

	public override int GetHashCode()
	{
		int hashCode = EqualityComparer<E>.Default.GetHashCode(this.Error!);

		if (this.value == null) { return hashCode; }

		unchecked
		{
			hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(this.value);
			return hashCode;
		}
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as Expected<T, E>);
	}
}
