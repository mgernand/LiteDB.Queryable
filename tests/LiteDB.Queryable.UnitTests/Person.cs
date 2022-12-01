namespace LiteDB.Queryable.UnitTests
{
	using System;

	public class Person : IComparable<Person>
	{
		public ObjectId Id { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

		public int Height { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.Name} ({this.Age})";
		}

		/// <inheritdoc />
		public int CompareTo(Person other)
		{
			if(ReferenceEquals(this, other))
			{
				return 0;
			}

			if(ReferenceEquals(null, other))
			{
				return 1;
			}

			return this.Age.CompareTo(other.Age);
		}
	}
}
