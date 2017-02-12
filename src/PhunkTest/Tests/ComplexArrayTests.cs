using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public static class ComplexArrayTests
	{
		private static dynamic Engine;

		[SetUp]
		public static void Setup()
		{
			Engine = new Engine();

			Engine("a = Array()");

			//Setup
			Engine(new[]
			{
				"run = () =>",
				"    for (i=0;i<10;i++)",
				"        a.Push(Number(i))",
				"run()",
			});
		}

		[TearDown]
		public static void TearDown()
		{
			Engine = null;
		}



		[Test]
		public static void For()
		{
			//Action
			Engine("a = a.ForAll(x=>x = x + 2)");

			Assert.That((int[])Engine("a"),
				Is.EquivalentTo(new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }));
		}

		[Test]
		public static void Where()
		{
			Engine("a = a.Where(x=>x>5)");

			Assert.That((int[])Engine("a"), Is.EquivalentTo(new[] { 6, 7, 8, 9 }));
		}

		[Test]
		public static void Find()
		{
			Engine("a.Get(2).Test = true");

			Engine("b = a.Find(x=>x.Test)");

			Assert.That((int)Engine("b"), Is.EqualTo(2));
		}

		[Test]
		public static void Remove()
		{
			Engine("a.Remove(x=>x == 2)");

			Assert.That((int[])Engine("a"),
				Is.EquivalentTo(new[] { 0, 1, 3, 4, 5, 6, 7, 8, 9 }));
		}
	}
}