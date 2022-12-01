namespace LiteDB.Queryable.UnitTests
{
	public class Company
	{
		public ObjectId Id { get; set; }

		public string Name { get; set; }

		public Person Owner { get; set; }
	}
}
