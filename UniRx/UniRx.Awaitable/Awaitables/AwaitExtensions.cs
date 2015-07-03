#if NET40PLUS && !NET45PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AwaitExtensions))]

#else

using System;
using UniRx.Runtime.CompilerServices;
using UniRx;

/// <summary>
/// Provides extension methods for threading-related types.
/// </summary>
public static class AwaitExtensions
{
    /// <summary>Gets an awaiter used to await this <see cref="Task"/>.</summary>
    /// <typeparam name="TResult">Specifies the type of data returned by the task.</typeparam>
    /// <param name="task">The task to await.</param>
    /// <returns>An awaiter instance.</returns>
    public static AsyncSubjectAwaiter<TSource> GetAwaiter<TSource>(this AsyncSubject<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        return new AsyncSubjectAwaiter<TSource>(source);
    }

    public static AsyncSubjectAwaiter<TSource> GetAwaiter<TSource>(this IObservable<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        var s = new AsyncSubject<TSource>();
        source.Subscribe(s);
        return GetAwaiter(s);
    }
}

#endif
