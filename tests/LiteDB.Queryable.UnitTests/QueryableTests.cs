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
	using System.Linq;
	using System.Threading.Tasks;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class QueryableTests
	{
		private LiteDatabase database;
		private ILiteCollection<Person> peopleCollection;
		private ILiteCollection<Company> companiesCollection;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			BsonMapper.Global.Entity<Company>()
				.DbRef(x => x.Owner, "people");
		}

		[SetUp]
		public void Setup()
		{
			this.database = new LiteDatabase($"{Guid.NewGuid():N}.db");
			this.peopleCollection = this.database.GetCollection<Person>("people");
			this.companiesCollection = this.database.GetCollection<Company>("companies");

			this.peopleCollection.Insert(new Person
			{
				Name = "Thomas",
				Age = 30,
				Height = 170,
				Weight = 15
			});
			this.peopleCollection.Insert(new Person
			{
				Name = "Benjamin",
				Age = 25,
				Height = 170,
				Weight = 20
			});
			this.peopleCollection.Insert(new Person
			{
				Name = "Thomas",
				Age = 27,
				Height = 200,
				Weight = null
			});
			this.peopleCollection.Insert(new Person
			{
				Name = "Albert",
				Age = 27,
				Height = 180,
				Weight = 30
			});
			this.peopleCollection.Insert(new Person
			{
				Name = "Tim",
				Age = 40,
				Height = 160,
				Weight = 10
			});

			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person owner = queryable.First(x => x.Name == "Tim");

			queryable = this.peopleCollection.AsQueryable();
			List<Person> employees = queryable.Where(x => x.Name != "Tim").ToList();

			this.companiesCollection.Insert(new Company
			{
				Name = "ACME Inc.",
				Owner = owner,
				Employees = employees
			});
		}

		[TearDown]
		public void TearDown()
		{
			this.database?.Dispose();
			this.database = null;
			this.peopleCollection = null;
		}

		[Test]
		public void ShouldAllowMultipleExecuteForPaging()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			// Execute the query to get the total item count.
			int count = queryable.Count();
			count.Should().Be(5);

			// Then apply a skip/take to the queryable.
			queryable = queryable.Skip(2).Take(2);

			// Execute the query again with the skip/take.
			IList<Person> persons = queryable.ToList();
			persons.Should().NotBeNullOrEmpty();
			persons.Count.Should().Be(2);
		}

		[Test]
		public void ShouldCreateQueryable()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			queryable.Should().NotBeNull();
		}

		[Test]
		public void ShouldExecuteAny()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			bool result = queryable
				.Where(x => x.Name == "Albert")
				.Any();

			result.Should().BeTrue();
		}

		[Test]
		public void ShouldExecuteAverageWithoutSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Average();

			result.Should().Be(97 / 3.0);
		}

		[Test]
		public void ShouldExecuteAverageWithSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Average(x => x.Age);

			result.Should().Be(97 / 3.0);
		}

		[Test]
		public void ShouldExecuteAverageWithSelectorNullable()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Average(x => x.Weight)
				.GetValueOrDefault();

			result.Should().Be(25 / 2.0);
		}

		[Test]
		public void ShouldExecuteAverageWithoutSelectorNullable()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			double result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Weight)
				.Average()
				.GetValueOrDefault();

			result.Should().Be(25 / 2.0);
		}

		[Test]
		public void ShouldExecuteCount()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Count();

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteFirst()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.First();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public void ShouldExecuteFirstOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.FirstOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
		}

		[Test]
		public void ShouldExecuteLongCount()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			long result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.LongCount();

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteMaxWithoutSelect()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Max();

			result.Should().NotBeNull();
			result.Name.Should().Be("Tim");
			result.Age.Should().Be(40);
		}

		[Test]
		public void ShouldExecuteMaxWithoutSelectWithComparer()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Max(new HeightComparer());

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
			result.Height.Should().Be(200);
		}

		[Test]
		public void ShouldExecuteMaxWithoutSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Max();

			result.Should().Be(40);
		}

		[Test]
		public void ShouldExecuteMaxWithSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Max(x => x.Age);

			result.Should().Be(40);
		}

		[Test]
		public void ShouldExecuteMinWithoutSelect()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Min();

			result.Should().NotBeNull();
			result.Name.Should().Be("Thomas");
			result.Age.Should().Be(27);
		}

		[Test]
		public void ShouldExecuteMinWithoutSelectWithComparer()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Min(new HeightComparer());

			result.Should().NotBeNull();
			result.Name.Should().Be("Tim");
			result.Height.Should().Be(160);
		}

		[Test]
		public void ShouldExecuteMinWithoutSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Min();

			result.Should().Be(27);
		}

		[Test]
		public void ShouldExecuteMinWithSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Min(x => x.Age);

			result.Should().Be(27);
		}

		[Test]
		public void ShouldExecuteMultipleWhere()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.Where(x => x.Name == "Thomas")
				.Where(x => x.Age == 30)
				.ToList();

			result.Should().HaveCount(1);
		}

		[Test]
		public void ShouldExecuteOrderBy()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.OrderBy(x => x.Age)
				.ToList();

			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public void ShouldExecuteOrderByDescending()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.OrderByDescending(x => x.Age)
				.ToList();

			result.First().Name.Should().Be("Tim");
		}

		[Test]
		public void ShouldExecuteRootToArray()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person[] result = queryable.ToArray();

			result.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteRootToArrayAsync()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person[] result = await queryable.ToArrayAsync();

			result.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteRootToDictionary()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Dictionary<string, string> result = queryable.ToDictionary(x => x.Id.ToString(), x => x.Name);

			result.Values.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteRootToList()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable.ToList();

			result.Should().HaveCount(5);
		}

		[Test]
		public async Task ShouldExecuteRootToListAsync()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = await queryable.ToListAsync();

			result.Should().HaveCount(5);
		}

		[Test]
		public void ShouldExecuteSingle()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.Single();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteSingleOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.SingleOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteSumWithoutSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Age)
				.Sum();

			result.Should().Be(97);
		}

		[Test]
		public void ShouldExecuteSumWithSelector()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Sum(x => x.Age);

			result.Should().Be(97);
		}

		[Test]
		public void ShouldExecuteSumWithoutSelectorNullable()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Select(x => x.Weight)
				.Sum()
				.GetValueOrDefault();

			result.Should().Be(25);
		}

		[Test]
		public void ShouldExecuteSumWithSelectorNullable()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable
				.Where(x => x.Name.StartsWith("T"))
				.Sum(x => x.Weight)
				.GetValueOrDefault();

			result.Should().Be(25);
		}

		[Test]
		public void ShouldExecuteWhere()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.Where(x => x.Name == "Thomas")
				.ToList();

			result.Should().HaveCount(2);
		}

		[Test]
		public void ShouldExecuteWhereFirst()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.First();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereFirstOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Albert")
				.FirstOrDefault();

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideAny()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			bool result = queryable.Any(x => x.Name == "Albert");

			result.Should().BeTrue();
		}

		[Test]
		public void ShouldExecuteWhereInsideCount()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			int result = queryable.Count(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteWhereInsideFirst()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.First(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideFirstOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.FirstOrDefault(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideLongCount()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			long result = queryable.LongCount(x => x.Name.StartsWith("T"));

			result.Should().Be(3);
		}

		[Test]
		public void ShouldExecuteWhereInsideSingle()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.Single(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWhereInsideSingleOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable.SingleOrDefault(x => x.Name == "Albert");

			result.Should().NotBeNull();
			result.Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWithSkip()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.Skip(1)
				.ToList();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Benjamin");
		}

		[Test]
		public void ShouldExecuteWithSkipTake()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.Take(2)
				.Skip(2)
				.ToList();

			result.Should().HaveCount(2);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldExecuteWithTake()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<Person> result = queryable
				.Take(4)
				.ToList();

			result.Should().HaveCount(4);
			result.First().Name.Should().Be("Thomas");
			result.Last().Name.Should().Be("Albert");
		}

		[Test]
		public void ShouldNotThrowOnEmptyResultFirstOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Sabrina")
				.FirstOrDefault();

			result.Should().BeNull();
		}

		[Test]
		public void ShouldNotThrowOnEmptyResultSingleOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Person result = queryable
				.Where(x => x.Name == "Heinz")
				.SingleOrDefault();

			result.Should().BeNull();
		}

		[Test]
		public void ShouldThrowForMultipleOrderBy()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.OrderBy(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public void ShouldThrowForMultipleOrderByDescending()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			Action action = () => queryable
				.OrderByDescending(x => x.Age)
				.OrderByDescending(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("Multiple OrderBy is not supported.");
		}

		[Test]
		public void ShouldThrowForThenBy()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.ThenBy(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public void ShouldThrowForThenByDescending()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();

			Action action = () => queryable
				.OrderBy(x => x.Age)
				.ThenByDescending(x => x.Name)
				.ToList();

			action.Should().Throw<NotSupportedException>().WithMessage("ThenBy is not supported.");
		}

		[Test]
		public void ShouldThrowOnEmptyResultFirst()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Sabrina")
				.First();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldThrowOnEmptyResultSingle()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Thomas")
				.Single();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldThrowOnMultipleResultSingleOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			Action action = () => queryable
				.Where(x => x.Name == "Thomas")
				.SingleOrDefault();

			action.Should().Throw<InvalidOperationException>();
		}

		[Test]
		public void ShouldIncludeReferencedEntity()
		{
			IQueryable<Company> queryable = this.companiesCollection.AsQueryable();
			Company result = queryable
				.Include(x => x.Owner)
				.First(x => x.Name.StartsWith("ACME"));

			result.Should().NotBeNull();
			result.Owner.Should().NotBeNull();
			result.Owner.Name.Should().NotBeNullOrWhiteSpace().And.Subject.Should().Be("Tim");
		}

		[Test]
		public void ShouldIncludeReferencedEntities()
		{
			IQueryable<Company> queryable = this.companiesCollection.AsQueryable();
			List<Company> results = queryable
				.Include(x => x.Employees)
				.ToList();

			results.Should().HaveCountGreaterThan(0);
			Company result = results[0];

			result.Should().NotBeNull();
			result.Employees.Should().NotBeNullOrEmpty();
			result.Employees.Should().HaveCount(4);
		}
		[Test]
		public void ShouldSelectValueFirst()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			string result = queryable
				.Where(x => x.Age == 40)
				.Select(x => x.Name)
				.First();

			result.Should().NotBeNullOrWhiteSpace();
			result.Should().Be("Tim");
		}

		[Test]
		public void ShouldSelectValueFirstOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			string result = queryable
				.Where(x => x.Age == 40)
				.Select(x => x.Name)
				.FirstOrDefault();

			result.Should().NotBeNullOrWhiteSpace();
			result.Should().Be("Tim");
		}

		[Test]
		public void ShouldSelectValueSingle()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			string result = queryable
				.Where(x => x.Age == 40)
				.Select(x => x.Name)
				.Single();

			result.Should().NotBeNullOrWhiteSpace();
			result.Should().Be("Tim");
		}

		[Test]
		public void ShouldSelectValueSingleOrDefault()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			string result = queryable
				.Where(x => x.Age == 40)
				.Select(x => x.Name)
				.SingleOrDefault();

			result.Should().NotBeNullOrWhiteSpace();
			result.Should().Be("Tim");
		}

		[Test]
		public void ShouldSelectMultipleValues()
		{
			IQueryable<Person> queryable = this.peopleCollection.AsQueryable();
			List<string> result = queryable
				.Where(x => x.Age == 40)
				.Select(x => x.Name)
				.ToList();

			result.Should().NotBeNullOrEmpty();
			result.Count.Should().Be(1);
		}
	}
}
