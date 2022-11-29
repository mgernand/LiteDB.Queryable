// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable MultipleOrderBy
// ReSharper disable ReplaceWithSingleCallToFirst
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToSingle
// ReSharper disable ReplaceWithSingleCallToSingleOrDefault
// ReSharper disable ReplaceWithSingleCallToCount
// ReSharper disable ReplaceWithSingleCallToAny

namespace LiteDB.Queryable.UnitTests
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using FluentAssertions;

	[TestFixture]
	public class QueryableTests
	{
		private LiteDatabase database;
		private ILiteCollection<Person> collection;

		[SetUp]
		public void Setup()
        {
			this.database = new LiteDatabase("test.db");
			this.collection = this.database.GetCollection<Person>();

			this.collection.Insert(new Person
			{
				Name = "Thomas",
				Age = 30
			});
			this.collection.Insert(new Person
			{
				Name = "Benjamin",
				Age = 25
			});
			this.collection.Insert(new Person
			{
				Name = "Thomas",
				Age = 27
			});
			this.collection.Insert(new Person
			{
				Name = "Albert",
				Age = 27
			});
			this.collection.Insert(new Person
			{
				Name = "Tim",
				Age = 40
			});
		}

		[TearDown]
		public void TearDown()
		{
			this.database?.Dispose();
			this.database = null;
			this.collection = null;

			File.Delete("test.db");
		}

		[Test]
		public void ShouldCreateQueryable()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			queryable.Should().NotBeNull();
		}

		[Test]
		public void ShouldExecuteRootToList()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable.ToList();

			result.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteRootToArray()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person[] result = queryable.ToArray();

			result.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteRootToDictionary()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Dictionary<string, string> result = queryable.ToDictionary(x => x.Id.ToString(), x => x.Name);

			result.Values.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteWhere()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.Where(x => x.Name == "Thomas")
				.ToList();

			result.Should().HaveCount(2);
		}

		[Test]
		public void ShouldExecuteMultipleWhere()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.Where(x => x.Name == "Thomas")
				.Where(x => x.Age == 30)
				.ToList();

			result.Should().HaveCount(1);
		}

		[Test]
		public void ShouldExecuteOrderBy()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.OrderBy(x => x.Age)
				.ToList();

			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public void ShouldExecuteOrderByDescending()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.OrderByDescending(x => x.Age)
				.ToList();

			result.First().Name.Should().Be("Tim");
		}

		[Test]
		public void ShouldThrowForMultipleOrderBy()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.OrderBy(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public void ShouldThrowForMultipleOrderByDescending()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Action action = () => queryable
				.OrderByDescending(x => x.Age)
				.OrderByDescending(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public void ShouldThrowForThenBy()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.ThenBy(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public void ShouldThrowForThenByDescending()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.ThenByDescending(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public void ShouldExecuteWithSkip()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.Skip(1)
				.ToList();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public void ShouldExecuteWithTake()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.Take(4)
				.ToList();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWithSkipTake()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable
				.Take(2)
				.Skip(2)
				.ToList();

			result.Should().HaveCount(2);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteFirst()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.First();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public void ShouldExecuteWhereFirst()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.First();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideFirst()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.First(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldThrowOnEmptyResultFirst()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Sabrina")
				.First();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldExecuteFirstOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.FirstOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public void ShouldExecuteWhereFirstOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.FirstOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideFirstOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.FirstOrDefault(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldNotThrowOnEmptyResultFirstOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Sabrina")
				.FirstOrDefault();

			result.Should().BeNull();
		}

		[Test]
		public void ShouldExecuteSingle()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.Single();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideSingle()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.Single(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldThrowOnEmptyResultSingle()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Thomas")
				.Single();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldExecuteSingleOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.SingleOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideSingleOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable.SingleOrDefault(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldNotThrowOnEmptyResultSingleOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Heinz")
				.SingleOrDefault();

			result.Should().BeNull();
		}

		[Test]
		public void ShouldThrowOnMultipleResultSingleOrDefault()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Thomas")
				.SingleOrDefault();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldExecuteCount()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Count();

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteWhereInsideCount()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable.Count(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteLongCount()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			long result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.LongCount();

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteWhereInsideLongCount()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			long result = queryable.LongCount(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteAny()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			bool result = queryable
				.Where(x => x.Name == "Albert")
				.Any();

			result.Should().BeTrue();
		}

		[Test]
		public void ShouldExecuteWhereInsideAny()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			bool result = queryable.Any(x => x.Name == "Albert");

			result.Should().BeTrue();
		}

		[Test]
		public void ShouldExecuteSumWithSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Sum(x => x.Age);

			result.Should().Be(97);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteSumWithoutSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Sum();

			result.Should().Be(97);
		}

		[Test]
		public void ShouldExecuteAverageWithSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Average(x => x.Age);

			result.Should().Be(97 / 3.0);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteAverageWithoutSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Average();

			result.Should().Be(97 / 3.0);
		}

		[Test]
		public void ShouldExecuteMinWithSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Min(x => x.Age);

			result.Should().Be(27);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteMinWithoutSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Min();

			result.Should().Be(27);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteMinWithoutSelect()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Min();

			result.Should().Be(null);
		}

		[Test]
		public void ShouldExecuteMaxWithSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Max(x => x.Age);

			result.Should().Be(40);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteMaxWithoutSelector()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Max();

			result.Should().Be(40);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public void ShouldExecuteMaxWithoutSelect()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Max();

			result.Should().Be(null);
		}
	}
}