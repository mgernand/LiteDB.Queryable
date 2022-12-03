namespace LiteDB.Queryable.UnitTests
{
	using System.Runtime.CompilerServices;
	using System.Threading.Tasks;

	/// <summary>
	///     Contains extension methods for <see cref="ConfiguredTaskAwaitable" /> type.
	/// </summary>
	public static class AwaitableExtensions
	{
		/// <summary>
		///     Simple helper that just awaits the result of the <see cref="ConfiguredTaskAwaitable" />
		///     and returns the data or the default value if the result was <c>null</c>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="awaitable"></param>
		/// <returns></returns>
		public static async Task<T> GetValueOrDefault<T>(this ConfiguredTaskAwaitable<T?> awaitable)
			where T : struct
		{
			T? value = await awaitable;
			return value.GetValueOrDefault();
		}
	}
}
