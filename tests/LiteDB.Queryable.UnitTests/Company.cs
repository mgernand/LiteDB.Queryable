namespace LiteDB.Queryable.UnitTests
{
	using System.Collections.Generic;

	public class Company
	{
		public ObjectId Id { get; set; }

		public string Name { get; set; }

		public Person Owner { get; set; }

		public IList<Person> Employees { get; set; }
	}
}
