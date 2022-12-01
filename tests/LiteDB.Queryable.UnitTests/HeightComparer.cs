namespace LiteDB.Queryable.UnitTests
{
	using System.Collections.Generic;

	public class HeightComparer : IComparer<Person>
	{
		public int Compare(Person x, Person y)
		{
			if(ReferenceEquals(x, y))
			{
				return 0;
			}

			if(ReferenceEquals(null, y))
			{
				return 1;
			}

			if(ReferenceEquals(null, x))
			{
				return -1;
			}

			return x.Height.CompareTo(y.Height);
		}
	}
}
