// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// From: https://github.com/dotnet/efcore/blob/main/src/EFCore/Query/IIncludableQueryable.cs

namespace LiteDB.Queryable
{
	using System.Linq;
	using JetBrains.Annotations;

	/// <summary>
	///     Supports queryable Include chaining operators.
	/// </summary>
	/// <typeparam name="TEntity">The entity type.</typeparam>
	/// <typeparam name="TProperty">The property type.</typeparam>
	[PublicAPI]
	public interface IIncludableQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
	{
	}
}
