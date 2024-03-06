// ReSharper disable ReplaceWithSingleCallToCount

namespace LiteDB.Queryable.UnitTests.Issues
{
	using NUnit.Framework;
	using System.Linq;
	using System;

	/// <summary>
	///		See: https://github.com/mgernand/LiteDB.Queryable/issues/22 (Thanks @ismailbennani)
	/// </summary>
	[TestFixture]
	public class Issue22
	{
		private LiteDatabase database;
		private ILiteCollection<Person> collection;

		[SetUp]
		public void Setup()
		{
			this.database = new LiteDatabase($"{Guid.NewGuid():N}.db");
			this.collection = this.database.GetCollection<Person>("people");

			this.collection.Insert(new Person
			{
				Name = "Thomas",
				Age = 30,
				Height = 170,
				Weight = 15
			});
			this.collection.Insert(new Person
			{
				Name = "Benjamin",
				Age = 25,
				Height = 170,
				Weight = 20
			});
			this.collection.Insert(new Person
			{
				Name = "Thomas",
				Age = 27,
				Height = 200,
				Weight = null
			});
			this.collection.Insert(new Person
			{
				Name = "Albert",
				Age = 27,
				Height = 180,
				Weight = 30
			});
			this.collection.Insert(new Person
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
		public void Reproduction()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			int firstResult = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Count();
			Console.WriteLine(firstResult);

			int secondResult = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Count();
			Console.WriteLine(secondResult);
		}
	}
}
