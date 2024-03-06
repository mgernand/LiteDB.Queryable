namespace LiteDB.Queryable.UnitTests
{
	using System.Collections.Generic;
	using System.IO;
	using NUnit.Framework;

	[SetUpFixture]
	public class GlobalFixture
	{
		[OneTimeSetUp]
		public void SetUpFixture()
		{
			DeleteDatabaseFiles();
		}

		[OneTimeTearDown]
		public void TearDownFixture()
		{
			DeleteDatabaseFiles();
		}

		private static void DeleteDatabaseFiles()
		{
			string currentDirectory = Directory.GetCurrentDirectory();
			IEnumerable<string> files = Directory.EnumerateFiles(currentDirectory, "*.db");
			foreach(string file in files)
			{
				if(File.Exists(file))
				{
					File.Delete(file);
				}
			}
		}
	}
}
