using System;
using System.Linq.Expressions;

namespace TinyValidations;

public sealed class ValidationRules<T>
{
    public void Required<TValue>(Expression<Func<T, TValue>> member, string? message = null) { }

    public void HasText(Expression<Func<T, string?>> member, string? message = null) { }

    public void NotNull<TValue>(Expression<Func<T, TValue>> member, string? message = null) { }

    public void HasItems<TItem>(Expression<Func<T, System.Collections.Generic.IEnumerable<TItem>?>> member, string? message = null) { }

    public void Email(Expression<Func<T, string?>> member, string? message = null) { }

    public void TextLengthAtLeast(Expression<Func<T, string?>> member, int length, string? message = null) { }

    public void TextLengthAtMost(Expression<Func<T, string?>> member, int length, string? message = null) { }

    public void Above<TValue>(Expression<Func<T, TValue>> member, TValue value, string? message = null) { }

    public void AtLeast<TValue>(Expression<Func<T, TValue>> member, TValue value, string? message = null) { }

    public void Below<TValue>(Expression<Func<T, TValue>> member, TValue value, string? message = null) { }

    public void AtMost<TValue>(Expression<Func<T, TValue>> member, TValue value, string? message = null) { }

    public void Matches(Expression<Func<T, string?>> member, string pattern, string? message = null) { }

    public void Use<TRule>() where TRule : IAsyncValidationRule<T> { }
}
