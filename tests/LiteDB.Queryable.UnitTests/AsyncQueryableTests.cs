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
	using System.Threading.Tasks;
	using FluentAssertions;
	using LiteDB.Async;

	[TestFixture]
	public class AsyncQueryableTests
	{
		private LiteDatabaseAsync database;
		private ILiteCollectionAsync<Person> collection;

		[SetUp]
		public async Task Setup()
		{
			this.database = new LiteDatabaseAsync("test.db");
			this.collection = this.database.GetCollection<Person>();

			await this.collection.InsertAsync(new Person
			{
				Name = "Thomas",
				Age = 30
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Benjamin",
				Age = 25
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Thomas",
				Age = 27
			});
			await this.collection.InsertAsync(new Person
			{
				Name = "Albert",
				Age = 27
			});
			await this.collection.InsertAsync(new Person
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
		public void ShouldExecuteRootToListOnAsyncCollection()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = queryable.ToList();

			result.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteRootToArrayOnAsyncCollection()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person[] result = queryable.ToArray();

			result.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteRootToListAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable.ToListAsync();

			result.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteRootToArrayAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person[] result = await queryable.ToArrayAsync();

			result.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteRootToDictionaryAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Dictionary<string, string> result = await queryable.ToDictionaryAsync(x => x.Id.ToString(), x => x.Name);

			result.Values.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteWhereAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.Where(x => x.Name == "Thomas")
				.ToListAsync();

			result.Should().HaveCount(2);
		}

		[Test]
		public async Task ShouldExecuteMultipleWhereAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.Where(x => x.Name == "Thomas")
				.Where(x => x.Age == 30)
				.ToListAsync();

			result.Should().HaveCount(1);
		}

		[Test]
		public async Task ShouldExecuteOrderByAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.OrderBy(x => x.Age)
				.ToListAsync();

			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public async Task ShouldExecuteOrderByDescendingAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.OrderByDescending(x => x.Age)
				.ToListAsync();

			result.First().Name.Should().Be("Tim");
		}

		[Test]
		public async Task ShouldThrowForMultipleOrderByAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Func<Task> action = async () => await queryable
				.OrderBy(x => x.Age)
				.OrderBy(x => x.Name)
				.ToListAsync();

			await action.Should().ThrowAsync<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public async Task ShouldThrowForMultipleOrderByDescendingAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Func<Task> action = async () => await queryable
				.OrderByDescending(x => x.Age)
				.OrderByDescending(x => x.Name)
				.ToListAsync();

			await action.Should().ThrowAsync<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public async Task ShouldThrowForThenByAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Func<Task> action = async () => await queryable
				.OrderBy(x => x.Age)
				.ThenBy(x => x.Name)
				.ToListAsync();

			await action.Should().ThrowAsync<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public async Task ShouldThrowForThenByDescendingAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();

			Func<Task> action = async () => await queryable
				.OrderBy(x => x.Age)
				.ThenByDescending(x => x.Name)
				.ToListAsync();

			await action.Should().ThrowAsync<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public async Task ShouldExecuteWithSkipAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.Skip(1)
				.ToListAsync();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public async Task ShouldExecuteWithTakeAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.Take(4)
				.ToListAsync();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteWithSkipTakeAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			List<Person> result = await queryable
				.Take(2)
				.Skip(2)
				.ToListAsync();

			result.Should().HaveCount(2);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteFirstAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.FirstAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public async Task ShouldExecuteWhereFirstAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Albert")
				.FirstAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteWhereInsideFirstAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.FirstAsync(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldThrowOnEmptyResultFirstAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Func<Task> action = async () => await queryable
				.Where(x => x.Name == "Sabrina")
				.FirstAsync();

			await action.Should().ThrowAsync<InvalidOperationException>();
		}

		[Test]
		public async Task ShouldExecuteFirstOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.FirstOrDefaultAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public async Task ShouldExecuteWhereFirstOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Albert")
				.FirstOrDefaultAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteWhereInsideFirstOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.FirstOrDefaultAsync(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldNotThrowOnEmptyResultFirstOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Sabrina")
				.FirstOrDefaultAsync();

			result.Should().BeNull();
		}

		[Test]
		public async Task ShouldExecuteSingleAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Albert")
				.SingleAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteWhereInsideSingleAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.SingleAsync(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldThrowOnEmptyResultSingleAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Func<Task> action = async () => await queryable
				.Where(x => x.Name == "Thomas")
				.SingleAsync();

			await action.Should().ThrowAsync<InvalidOperationException>();
		}

		[Test]
		public async Task ShouldExecuteSingleOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Albert")
				.SingleOrDefaultAsync();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldExecuteWhereInsideSingleOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable.SingleOrDefaultAsync(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public async Task ShouldNotThrowOnEmptyResultSingleOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name == "Heinz")
				.SingleOrDefaultAsync();

			result.Should().BeNull();
		}

		[Test]
		public async Task ShouldThrowOnMultipleResultSingleOrDefaultAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Func<Task> action = async () => await queryable
				.Where(x => x.Name == "Thomas")
				.SingleOrDefaultAsync();

			await action.Should().ThrowAsync<InvalidOperationException>();
		}

		[Test]
		public async Task ShouldExecuteCountAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.CountAsync();

			result.Should().Be(3);
		}

		[Test]
		public async Task ShouldExecuteWhereInsideCountAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable.CountAsync(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public async Task ShouldExecuteLongCountAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			long result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.LongCountAsync();

			result.Should().Be(3);
		}

		[Test]
		public async Task ShouldExecuteWhereInsideLongCountAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			long result = await queryable.LongCountAsync(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public async Task ShouldExecuteAnyAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			bool result = await queryable
				.Where(x => x.Name == "Albert")
				.AnyAsync();

			result.Should().BeTrue();
		}

		[Test]
		public async Task ShouldExecuteWhereInsideAnyAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			bool result = await queryable.AnyAsync(x => x.Name == "Albert");

			result.Should().BeTrue();
		}

		[Test]
		public async Task ShouldExecuteSumWithSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.SumAsync(x => x.Age);

			result.Should().Be(97);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteSumWithoutSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.SumAsync();

			result.Should().Be(97);
		}

		[Test]
		public async Task ShouldExecuteAverageWithSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			double result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.AverageAsync(x => x.Age);

			result.Should().Be(97 / 3.0);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteAverageWithoutSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			double result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.AverageAsync();

			result.Should().Be(97 / 3.0);
		}

		[Test]
		public async Task ShouldExecuteMinWithSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.MinAsync(x => x.Age);

			result.Should().Be(27);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteMinWithoutSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.MinAsync();

			result.Should().Be(27);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteMinWithoutSelectAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.MinAsync();

			result.Should().Be(null);
		}

		[Test]
		public async Task ShouldExecuteMaxWithSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.MaxAsync(x => x.Age);

			result.Should().Be(40);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteMaxWithoutSelectorAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			int result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.MaxAsync();

			result.Should().Be(40);
		}

		[Test]
		[Ignore("Ignore until Select() is implemented.")]
		public async Task ShouldExecuteMaxWithoutSelectAsync()
		{
			IQueryable<Person> queryable = this.collection.AsQueryable();
			Person result = await queryable
				.Where(x => x.Name.StartsWith("T"))
				.MaxAsync();

			result.Should().Be(null);
		}
	}
}
