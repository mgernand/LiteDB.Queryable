namespace LiteDB.Queryable.UnitTests
{
	public class Person
	{
		public ObjectId Id { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return $"{this.Name} ({this.Age})";
		}
	}
}
