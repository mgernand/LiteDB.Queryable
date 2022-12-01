// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// From: https://github.com/dotnet/efcore/blob/main/src/EFCore/Extensions/EntityFrameworkQueryableExtensions.cs

namespace LiteDB.Queryable
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Linq;
	using System.Linq.Expressions;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using JetBrains.Annotations;

	/// <summary>
	///     Extension methods to execute actions on an <see cref="IQueryable" /> object asynchronously.
	/// </summary>
	[PublicAPI]
	public static class LiteQueryableExtensions
	{
		#region First/FirstOrDefault

		/// <summary>
		///     Asynchronously returns the first element of a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable" /> to return the first element of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the first element in <paramref name="source" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> FirstAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the first element of a sequence that satisfies a specified condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the first element of.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the first element in <paramref name="source" /> that passes the test in
		///     <paramref name="predicate" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     <para>
		///         No element satisfies the condition in <paramref name="predicate" />
		///     </para>
		///     <para>
		///         -or -
		///     </para>
		///     <para>
		///         <paramref name="source" /> contains no elements.
		///     </para>
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> FirstAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstWithPredicate, source, predicate, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the first element of a sequence, or a default value if the sequence contains no elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the first element of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains <see langword="default" /> ( <typeparamref name="TSource" /> ) if
		///     <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the first element of a sequence that satisfies a specified condition
		///     or a default value if no such element is found.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the first element of.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains <see langword="default" /> ( <typeparamref name="TSource" /> ) if
		///     <paramref name="source" />
		///     is empty or if no element passes the test specified by <paramref name="predicate" />, otherwise, the first
		///     element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.FirstOrDefaultWithPredicate, source, predicate, cancellationToken);
		}

		#endregion

		#region Single/SingleOrDefault

		/// <summary>
		///     Asynchronously returns the only element of a sequence, and throws an exception
		///     if there is not exactly one element in the sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the single element of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the single element of the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException">
		///     <para>
		///         <paramref name="source" /> contains more than one elements.
		///     </para>
		///     <para>
		///         -or-
		///     </para>
		///     <para>
		///         <paramref name="source" /> contains no elements.
		///     </para>
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> SingleAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.SingleWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the only element of a sequence that satisfies a specified condition,
		///     and throws an exception if more than one such element exists.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the single element of.</param>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the single element of the input sequence that satisfies the condition in
		///     <paramref name="predicate" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     <para>
		///         No element satisfies the condition in <paramref name="predicate" />.
		///     </para>
		///     <para>
		///         -or-
		///     </para>
		///     <para>
		///         More than one element satisfies the condition in <paramref name="predicate" />.
		///     </para>
		///     <para>
		///         -or-
		///     </para>
		///     <para>
		///         <paramref name="source" /> contains no elements.
		///     </para>
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> SingleAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.SingleWithPredicate, source, predicate, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the only element of a sequence, or a default value if the sequence is empty;
		///     this method throws an exception if there is more than one element in the sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the single element of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the single element of the input sequence, or <see langword="default" /> (
		///     <typeparamref name="TSource" />)
		///     if the sequence contains no elements.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains more than one element.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.SingleOrDefaultWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the only element of a sequence that satisfies a specified condition or
		///     a default value if no such element exists; this method throws an exception if more than one element
		///     satisfies the condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to return the single element of.</param>
		/// <param name="predicate">A function to test an element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the single element of the input sequence that satisfies the condition in
		///     <paramref name="predicate" />, or <see langword="default" /> ( <typeparamref name="TSource" /> ) if no such element
		///     is found.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		///     More than one element satisfies the condition in <paramref name="predicate" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<TSource>>(
				QueryableMethods.SingleOrDefaultWithPredicate, source, predicate, cancellationToken);
		}

		#endregion

		#region Count/LongCount

		/// <summary>
		///     Asynchronously returns the number of elements in a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to be counted.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the number of elements in the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int> CountAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<int>>(QueryableMethods.CountWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns the number of elements in a sequence that satisfy a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to be counted.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the number of elements in the sequence that satisfy the condition in the predicate
		///     function.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int> CountAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<int>>(QueryableMethods.CountWithPredicate, source, predicate, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns a <see cref="long" /> that represents the total number of elements in a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to be counted.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the number of elements in the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long> LongCountAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<long>>(QueryableMethods.LongCountWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously returns a <see cref="long" /> that represents the number of elements in a sequence
		///     that satisfy a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to be counted.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the number of elements in the sequence that satisfy the condition in the predicate
		///     function.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long> LongCountAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<long>>(QueryableMethods.LongCountWithPredicate, source, predicate, cancellationToken);
		}

		#endregion

		#region Any

		/// <summary>
		///     Asynchronously determines whether a sequence contains any elements.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to check for being empty.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains <see langword="true" /> if the source sequence contains any elements; otherwise,
		///     <see langword="false" />.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<bool> AnyAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<bool>>(QueryableMethods.AnyWithoutPredicate, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously determines whether any element of a sequence satisfies a condition.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> whose elements to test for a condition.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains <see langword="true" /> if any elements in the source sequence pass the test in the
		///     specified
		///     predicate; otherwise, <see langword="false" />.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="predicate" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<bool> AnyAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			if(predicate is null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}

			return ExecuteAsync<TSource, Task<bool>>(QueryableMethods.AnyWithPredicate, source, predicate, cancellationToken);
		}

		#endregion

		#region Sum

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal> SumAsync(
			this IQueryable<decimal> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<decimal, Task<decimal>>(QueryableMethods.GetSumWithoutSelector(typeof(decimal)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal?> SumAsync(
			this IQueryable<decimal?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<decimal?, Task<decimal?>>(
				QueryableMethods.GetSumWithoutSelector(typeof(decimal?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, decimal>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<decimal>>(
				QueryableMethods.GetSumWithSelector(typeof(decimal)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal?> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, decimal?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<decimal?>>(
				QueryableMethods.GetSumWithSelector(typeof(decimal?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int> SumAsync(
			this IQueryable<int> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<int, Task<int>>(QueryableMethods.GetSumWithoutSelector(typeof(int)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int?> SumAsync(
			this IQueryable<int?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<int?, Task<int?>>(QueryableMethods.GetSumWithoutSelector(typeof(int?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, int>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<int>>(QueryableMethods.GetSumWithSelector(typeof(int)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<int?> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, int?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<int?>>(
				QueryableMethods.GetSumWithSelector(typeof(int?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long> SumAsync(
			this IQueryable<long> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<long, Task<long>>(QueryableMethods.GetSumWithoutSelector(typeof(long)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long?> SumAsync(
			this IQueryable<long?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<long?, Task<long?>>(QueryableMethods.GetSumWithoutSelector(typeof(long?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, long>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<long>>(
				QueryableMethods.GetSumWithSelector(typeof(long)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<long?> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, long?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<long?>>(
				QueryableMethods.GetSumWithSelector(typeof(long?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> SumAsync(
			this IQueryable<double> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<double, Task<double>>(QueryableMethods.GetSumWithoutSelector(typeof(double)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> SumAsync(
			this IQueryable<double?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<double?, Task<double?>>(QueryableMethods.GetSumWithoutSelector(typeof(double?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, double>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double>>(
				QueryableMethods.GetSumWithSelector(typeof(double)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, double?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double?>>(
				QueryableMethods.GetSumWithSelector(typeof(double?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float> SumAsync(
			this IQueryable<float> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<float, Task<float>>(QueryableMethods.GetSumWithoutSelector(typeof(float)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the sum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the values in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float?> SumAsync(
			this IQueryable<float?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<float?, Task<float?>>(QueryableMethods.GetSumWithoutSelector(typeof(float?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, float>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<float>>(
				QueryableMethods.GetSumWithSelector(typeof(float)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the sum of the sequence of values that is obtained by invoking a projection function on
		///     each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the sum of the projected values..
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float?> SumAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, float?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<float?>>(
				QueryableMethods.GetSumWithSelector(typeof(float?)), source, selector, cancellationToken);
		}

		#endregion

		#region Average

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal> AverageAsync(
			this IQueryable<decimal> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<decimal, Task<decimal>>(
				QueryableMethods.GetAverageWithoutSelector(typeof(decimal)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal?> AverageAsync(
			this IQueryable<decimal?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<decimal?, Task<decimal?>>(
				QueryableMethods.GetAverageWithoutSelector(typeof(decimal?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, decimal>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<decimal>>(
				QueryableMethods.GetAverageWithSelector(typeof(decimal)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<decimal?> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, decimal?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<decimal?>>(
				QueryableMethods.GetAverageWithSelector(typeof(decimal?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync(
			this IQueryable<int> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<int, Task<double>>(QueryableMethods.GetAverageWithoutSelector(typeof(int)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync(
			this IQueryable<int?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<int?, Task<double?>>(QueryableMethods.GetAverageWithoutSelector(typeof(int?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, int>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double>>(
				QueryableMethods.GetAverageWithSelector(typeof(int)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, int?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double?>>(
				QueryableMethods.GetAverageWithSelector(typeof(int?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync(
			this IQueryable<long> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<long, Task<double>>(QueryableMethods.GetAverageWithoutSelector(typeof(long)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync(
			this IQueryable<long?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<long?, Task<double?>>(QueryableMethods.GetAverageWithoutSelector(typeof(long?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, long>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double>>(
				QueryableMethods.GetAverageWithSelector(typeof(long)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, long?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double?>>(
				QueryableMethods.GetAverageWithSelector(typeof(long?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync(
			this IQueryable<double> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<double, Task<double>>(
				QueryableMethods.GetAverageWithoutSelector(typeof(double)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync(
			this IQueryable<double?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<double?, Task<double?>>(
				QueryableMethods.GetAverageWithoutSelector(typeof(double?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, double>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double>>(
				QueryableMethods.GetAverageWithSelector(typeof(double)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<double?> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, double?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<double?>>(
				QueryableMethods.GetAverageWithSelector(typeof(double?)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float> AverageAsync(
			this IQueryable<float> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<float, Task<float>>(QueryableMethods.GetAverageWithoutSelector(typeof(float)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the sequence of values.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float?> AverageAsync(
			this IQueryable<float?> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<float?, Task<float?>>(
				QueryableMethods.GetAverageWithoutSelector(typeof(float?)), source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, float>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<float>>(
				QueryableMethods.GetAverageWithSelector(typeof(float)), source, selector, cancellationToken);
		}

		/// <summary>
		///     Asynchronously computes the average of a sequence of values that is obtained
		///     by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">A sequence of values of type <typeparamref name="TSource" />.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the average of the projected values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<float?> AverageAsync<TSource>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, float?>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<float?>>(
				QueryableMethods.GetAverageWithSelector(typeof(float?)), source, selector, cancellationToken);
		}

		#endregion

		#region Min

		/// <summary>
		///     Asynchronously returns the minimum value of a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the minimum value in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> MinAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.MinWithoutSelector, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously invokes a projection function on each element of a sequence and returns the minimum resulting value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TResult">
		///     The type of the value returned by the function represented by <paramref name="selector" />.
		/// </typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to determine the minimum of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the minimum value in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TResult> MinAsync<TSource, TResult>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, TResult>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<TResult>>(QueryableMethods.MinWithSelector, source, selector, cancellationToken);
		}

		#endregion

		#region Max

		/// <summary>
		///     Asynchronously returns the maximum value of a sequence.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the maximum value in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TSource> MaxAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, Task<TSource>>(QueryableMethods.MaxWithoutSelector, source, cancellationToken);
		}

		/// <summary>
		///     Asynchronously invokes a projection function on each element of a sequence and returns the maximum resulting value.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TResult">
		///     The type of the value returned by the function represented by <paramref name="selector" />.
		/// </typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> that contains the elements to determine the maximum of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains the maximum value in the sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="InvalidOperationException"><paramref name="source" /> contains no elements.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<TResult> MaxAsync<TSource, TResult>(
			this IQueryable<TSource> source,
			Expression<Func<TSource, TResult>> selector,
			CancellationToken cancellationToken = default)
		{
			if(selector is null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return ExecuteAsync<TSource, Task<TResult>>(QueryableMethods.MaxWithSelector, source, selector, cancellationToken);
		}

		#endregion

		#region ToList/Array

		/// <summary>
		///     Asynchronously creates a <see cref="List{T}" /> from an <see cref="IQueryable{T}" /> by enumerating it
		///     asynchronously.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create a list from.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains a <see cref="List{T}" /> that contains elements from the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static async Task<List<TSource>> ToListAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			List<TSource> list = new List<TSource>();
			await foreach(TSource item in source.ToAsyncEnumerable().WithCancellation(cancellationToken))
			{
				list.Add(item);
			}

			return list;
		}

		/// <summary>
		///     Asynchronously creates an array from an <see cref="IQueryable{T}" /> by enumerating it asynchronously.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create an array from.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains an array that contains elements from the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException"><paramref name="source" /> is <see langword="null" />.</exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static async Task<TSource[]> ToArrayAsync<TSource>(
			this IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return (await source.ToListAsync(cancellationToken).ConfigureAwait(false)).ToArray();
		}

		#endregion

		#region ToDictionary

		/// <summary>
		///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
		///     asynchronously according to a specified key selector function.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			CancellationToken cancellationToken = default)
			where TKey : notnull
		{
			return source.ToDictionaryAsync(keySelector, e => e, null, cancellationToken);
		}

		/// <summary>
		///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
		///     asynchronously according to a specified key selector function and a comparer.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{TKey}" /> to compare keys.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains a <see cref="Dictionary{TKey, TSource}" /> that contains selected keys and values.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="keySelector" /> is <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<Dictionary<TKey, TSource>> ToDictionaryAsync<TSource, TKey>(
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			IEqualityComparer<TKey> comparer,
			CancellationToken cancellationToken = default)
			where TKey : notnull
		{
			return source.ToDictionaryAsync(keySelector, e => e, comparer, cancellationToken);
		}

		/// <summary>
		///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
		///     asynchronously according to a specified key selector and an element selector function.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
		///     <typeparamref name="TElement" /> selected from the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is
		///     <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector,
			CancellationToken cancellationToken = default)
			where TKey : notnull
		{
			return source.ToDictionaryAsync(keySelector, elementSelector, null, cancellationToken);
		}

		/// <summary>
		///     Creates a <see cref="Dictionary{TKey, TValue}" /> from an <see cref="IQueryable{T}" /> by enumerating it
		///     asynchronously according to a specified key selector function, a comparer, and an element selector function.
		/// </summary>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector" />.</typeparam>
		/// <typeparam name="TElement">The type of the value returned by <paramref name="elementSelector" />.</typeparam>
		/// <param name="source">An <see cref="IQueryable{T}" /> to create a <see cref="Dictionary{TKey, TValue}" /> from.</param>
		/// <param name="keySelector">A function to extract a key from each element.</param>
		/// <param name="elementSelector">A transform function to produce a result element value from each element.</param>
		/// <param name="comparer">An <see cref="IEqualityComparer{TKey}" /> to compare keys.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
		/// <returns>
		///     A task that represents the asynchronous operation.
		///     The task result contains a <see cref="Dictionary{TKey, TElement}" /> that contains values of type
		///     <typeparamref name="TElement" /> selected from the input sequence.
		/// </returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="keySelector" /> or <paramref name="elementSelector" /> is
		///     <see langword="null" />.
		/// </exception>
		/// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
		public static async Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
			this IQueryable<TSource> source,
			Func<TSource, TKey> keySelector,
			Func<TSource, TElement> elementSelector,
			IEqualityComparer<TKey> comparer,
			CancellationToken cancellationToken = default)
			where TKey : notnull
		{
			if(keySelector is null)
			{
				throw new ArgumentNullException(nameof(keySelector));
			}

			if(elementSelector is null)
			{
				throw new ArgumentNullException(nameof(elementSelector));
			}

			Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(comparer);
			await foreach(TSource source1 in source.ToAsyncEnumerable().WithCancellation(cancellationToken))
			{
				dictionary.Add(keySelector(source1), elementSelector(source1));
			}

			return dictionary;
		}

		#endregion

		#region Include

		internal static readonly MethodInfo IncludeMethodInfo
			= typeof(LiteQueryableExtensions)
				.GetTypeInfo().GetDeclaredMethods(nameof(Include))
				.Single(m => 
					m.GetGenericArguments().Length == 2 && 
					m.GetParameters().Any(pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));

		/// <summary>
		///     Specifies related entities to include in the query results. The navigation property to be included is specified starting with the
		///     type of entity being queried (<typeparamref name="TEntity" />).
		/// </summary>
		/// <typeparam name="TEntity">The type of entity being queried.</typeparam>
		/// <typeparam name="TProperty">The type of the related entity to be included.</typeparam>
		/// <param name="source">The source query.</param>
		/// <param name="navigationPropertyPath">
		///     A lambda expression representing the navigation property to be included (<c>t => t.Property1</c>).
		/// </param>
		/// <returns>A new query with the related data included.</returns>
		/// <exception cref="ArgumentNullException">
		///     <paramref name="source" /> or <paramref name="navigationPropertyPath" /> is <see langword="null" />.
		/// </exception>
		public static IIncludableQueryable<TEntity, TProperty> Include<TEntity, TProperty>(
			this IQueryable<TEntity> source,
			Expression<Func<TEntity, TProperty>> navigationPropertyPath)
			where TEntity : class
		{
			if(navigationPropertyPath is null)
			{
				throw new ArgumentNullException(nameof(navigationPropertyPath));
			}

			return new IncludableQueryable<TEntity, TProperty>(
				source.Provider is LiteQueryProvider<TEntity>
					? source.Provider.CreateQuery<TEntity>(Expression.Call(
						instance: null,
						method: IncludeMethodInfo.MakeGenericMethod(typeof(TEntity), typeof(TProperty)),
						arguments: new[] { source.Expression, Expression.Quote(navigationPropertyPath) }))
					: source);
		}

		private sealed class IncludableQueryable<TEntity, TProperty> : IIncludableQueryable<TEntity, TProperty>, IAsyncEnumerable<TEntity>
		{
			private readonly IQueryable<TEntity> queryable;

			public IncludableQueryable(IQueryable<TEntity> queryable)
			{
				this.queryable = queryable;
			}

			public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
			{
				return ((IAsyncEnumerable<TEntity>)this.queryable).GetAsyncEnumerator(cancellationToken);
			}

			public IEnumerator<TEntity> GetEnumerator()
			{
				return this.queryable.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.GetEnumerator();
			}

			public Expression Expression => this.queryable.Expression;

			public Type ElementType => this.queryable.ElementType;

			public IQueryProvider Provider => this.queryable.Provider;
		}

		#endregion

		#region Execute

		private static TResult ExecuteAsync<TSource, TResult>(
			MethodInfo operatorMethodInfo,
			IQueryable<TSource> source,
			Expression expression,
			CancellationToken cancellationToken = default)
		{
			if(source.Provider is IAsyncQueryProvider provider)
			{
				if(operatorMethodInfo.IsGenericMethod)
				{
					operatorMethodInfo
						= operatorMethodInfo.GetGenericArguments().Length == 2
							? operatorMethodInfo.MakeGenericMethod(typeof(TSource), typeof(TResult).GetGenericArguments().Single())
							: operatorMethodInfo.MakeGenericMethod(typeof(TSource));
				}

				return provider.ExecuteAsync<TResult>(
					Expression.Call(
						null,
						operatorMethodInfo,
						expression == null
							? new[] { source.Expression }
							: new[] { source.Expression, expression }),
					cancellationToken);
			}

			throw new InvalidOperationException("The queryable provider is not async");
		}

		private static TResult ExecuteAsync<TSource, TResult>(
			MethodInfo operatorMethodInfo,
			IQueryable<TSource> source,
			LambdaExpression expression,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, Expression.Quote(expression), cancellationToken);
		}

		private static TResult ExecuteAsync<TSource, TResult>(
			MethodInfo operatorMethodInfo,
			IQueryable<TSource> source,
			CancellationToken cancellationToken = default)
		{
			return ExecuteAsync<TSource, TResult>(operatorMethodInfo, source, (Expression)null, cancellationToken);
		}

		#endregion
	}
}
