// ReSharper disable ReplaceWithSingleCallToCount

namespace LiteDB.Queryable.UnitTests.Issues
{
	using NUnit.Framework;
	using System.Linq;
	using System;
	using LiteDB.Async;
	using System.Threading.Tasks;

	/// <summary>
	///		See: https://github.com/mgernand/LiteDB.Queryable/issues/22 (Thanks @ismailbennani)
	/// </summary>
	[TestFixture]
	public class Issue22Async
	{
		private LiteDatabaseAsync database;
		private ILiteCollectionAsync<Person> collection;

		[SetUp]
		public async Task Setup()
		{
			this.database = new LiteDatabaseAsync($"{Guid.NewGuid():N}.db");
			this.collection = this.database.GetCollection<Person>("people");

			await this.collection.InsertAsync(new Person
			{
				Name = "Thomas",
				Age = 30,
				Height = 170,
				Weight = 15
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Benjamin",
				Age = 25,
				Height = 170,
				Weight = 20
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Thomas",
				Age = 27,
				Height = 200,
				Weight = null
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Albert",
				Age = 27,
				Height = 180,
				Weight = 30
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Tim",
				Age = 40,
				Height = 160,
				Weight = 10
			});
		}

		[TearDown]
		public void TearDown()
		{
			this.database?.Dispose();
			this.database = null;
			this.collection = null;
		}

		[Test]
		public async Task Reproduction()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			int firstResult = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.CountAsync();
			Console.WriteLine(firstResult);

			int secondResult = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.CountAsync();
			Console.WriteLine(secondResult);
		}
	}
}
