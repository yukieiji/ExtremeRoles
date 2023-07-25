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

public sealed class Expected<T> : IEquatable<Expected<T>>
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

	private readonly T? value;

	public Expected(T? value)
	{
		this.value = value;
	}

	public bool HasValue()
		=> this.value != null;

	public Expected<T> AndThen(Action<T> lamda)
	{
		if (this.value != null)
		{
			lamda.Invoke(this.value);
		}
		return this;
	}

	public Expected<T, E> AndThen<E>(Func<T, Expected<T, E>> lamda) where E : new()
		=> this.value != null ?
		lamda.Invoke(this.value) :
		new Expected<T, E>(new E());

	public Expected<X> Transform<X>(Func<T, X> lamda)
		=> this.value != null ?
		new Expected<X>(lamda.Invoke(this.Value)) :
		new Expected<X>(default(X));

	public Expected<X, E> Transform<X, E>(Func<T, X> lamda) where E : new()
		=> this.value != null ?
		new Expected<X, E>(lamda.Invoke(this.Value)) :
		new Expected<X, E>(new E());

	public bool Equals(Expected<T>? other)
	{
		if (other is null) { return false; }

		bool rightHasValue = this.HasValue();
		bool leftHasValue = other.HasValue();

		if (rightHasValue && leftHasValue)
		{
			return EqualityComparer<T>.Default.Equals(other.Value, this.Value);
		}
		else
		{
			return false;
		}
	}

	public static bool operator ==(Expected<T> left, Expected<T>? right)
		=> left.Equals(right);

	public static bool operator !=(Expected<T> left, Expected<T>? right)
		=> !left.Equals(right);

	public static implicit operator Expected<T>(T? v)
	{
		return new Expected<T>(v);
	}

	public override int GetHashCode()
		=> this.value == null ? 0 : EqualityComparer<T>.Default.GetHashCode(this.value);

	public override bool Equals(object? obj)
	{
		return Equals(obj as Expected<T>);
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

	private readonly T? value;

	public Expected(T? value)
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

	public Expected<T, E> AndThen(Action<T> lamda)
	{
		if (this.value != null)
		{
			lamda.Invoke(this.value);
		}
		return this;
	}

	public Expected<T, E> OrElse(Action<E> lamda)
	{
		if (this.value == null)
		{
			lamda.Invoke(this.Error);
		}
		return this;
	}

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

	public bool Equals(Expected<T>? other)
	{
		if (other is null) { return false; }

		bool rightHasValue = this.HasValue();
		bool leftHasValue = other.HasValue();

		if (rightHasValue && leftHasValue)
		{
			return EqualityComparer<T>.Default.Equals(other.Value, this.Value);
		}
		else
		{
			return false;
		}
	}

	public bool Equals(Expected<E>? other)
	{
		if (other is null) { return false; }

		bool rightHasValue = this.HasValue();
		bool leftHasValue = other.HasValue();

		if (!rightHasValue && leftHasValue)
		{
			return EqualityComparer<E>.Default.Equals(other.Value, this.Error);
		}
		else
		{
			return false;
		}
	}

	// 特殊テンプレート
	public static bool operator ==(Expected<T>? left, Expected<T, E> right)
		=> right.Equals(left);
	public static bool operator !=(Expected<T>? left, Expected<T, E> right)
		=> !right.Equals(left);

	// 特殊テンプレート
	public static bool operator ==(Expected<E>? left, Expected<T, E> right)
		=> right.Equals(left);
	public static bool operator !=(Expected<E>? left, Expected<T, E> right)
		=> !right.Equals(left);

	// 特殊テンプレート
	public static bool operator ==(Expected<T, E> left, Expected<E>? right)
		=> left.Equals(right);
	public static bool operator !=(Expected<T, E> left, Expected<E>? right)
		=> !left.Equals(right);

	// 特殊テンプレート
	public static bool operator ==(Expected<T, E> left, Expected<T>? right)
		=> left.Equals(right);
	public static bool operator !=(Expected<T, E> left, Expected<T>? right)
		=> !left.Equals(right);

	public static bool operator ==(Expected<T, E> left, Expected<T, E>? right)
		=> left.Equals(right);

	public static bool operator !=(Expected<T, E> left, Expected<T, E>? right)
		=> !left.Equals(right);

	public static implicit operator Expected<T, E>(T? v)
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
