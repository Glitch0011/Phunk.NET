using NUnit.Framework;
using System.Diagnostics;

namespace Phunk.Luan.Tests
{
	public static class ArrayTests
	{
        [Test]
        public static void AddItemOperator()
        {
            dynamic engine = new Engine();

            engine("test = Array()");
            engine("test += 1");
            engine("test += 3");

            Assert.That((int[])engine("test"),
                Is.EquivalentTo(new[] { 1, 3 }));
        }

        [Test]
		public static void Add()
		{
			dynamic engine = new Engine();

			engine("a = Array()");
			engine("a.Add(2)");

			Assert.That(engine("a.Length()") == 1);
		}

		[Test]
		public static void Push()
		{
			dynamic engine = new Engine();

			engine("a = Array()");
			engine("a.Push(2)");

			Assert.That(engine("a.Length()") == 1);
		}

		[Test]
		public static void PushTest()
		{
			dynamic engine = new Engine();

			engine("a = Array()");
			engine("a.Push(3)");

			Assert.That(engine("a.Length()") == 1);
		}
        
		[Test]
		public static void AddUnionOperator()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"b = Array()",
				"b += 1",
				"b += 2",

				"c = Array()",
				"c += 3",
				"c += 4",

				"d = b + c"
			});
			
			Assert.That((int[])engine("d"), Is.EquivalentTo(new[] {1, 2, 3, 4}));
		}

        [Test]
        public static void AddUnionOperatorInPlace()
        {
            dynamic engine = new Engine();

            engine(new[]
            {
                "b = Array()",
                "b += 1",
                "b += 2",

                "c = Array()",
                "c += 3",
                "c += 4",

                "b += c"
            });

            Assert.That((int[])engine("b"), Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
		public static void All()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"b = Array()",
				"b += true",
				"b += true",

				"c = Array()",
				"c += true",
				"c += false",

				"d = b.All(x=>x)",
				"e = c.All(x=>x)",
			});

			Assert.That((bool) engine("d"), Is.True);
			Assert.That((bool) engine("e"), Is.False);
		}
	}
}