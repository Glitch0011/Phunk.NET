using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	[TestFixture]
	public class ExampleTests
	{
		private dynamic d { get; set; }

		[SetUp]
		public void SetupEngine()
		{
			d = new Engine();
		}

		[TearDown]
		public void DisposeEngine()
		{
			d = null;
		}

		[Test]
		public void HelloWorld()
		{
			string output = null;

			((Engine)d).OnConsoleOutput += str => output = str;

			d("Console.Write(\"Hello, World!\")");

			Assert.That(output, Is.EqualTo("Hello, World!"));
		}

		[Test]
		public void Fibonacci()
		{
			d(new[]
			{
				"Fibonnaci = (n) =>",
				"    a = 0",
				"    b = 1",
				"    for (i = 0; i < n; i++)",
				"        temp = a",
				"        a = b",
				"        b = temp + a",
				"    return a",
				"",
				"run = () =>",
				"    vals = Array()",
				"    for (i = 0; i < 15; i++)",
				"        vals.Push(Fibonnaci(i))",
				"    return vals"
			});

			Assert.That((int[]) d("run()"),
				Is.EquivalentTo(new[] {0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377}));
		}

        [Test]
        public void ReferenceFunctionArgument()
        {
            d(new[]
            {
                "Add = (a) =>",
                "    a = a + 1",
                "    return a" 
            });

            Assert.That((int)d("Add(1)"), Is.EqualTo(2));
        }

		[Test]
		public void CustomKeyword()
		{
			d(new[]
			{
				"GetAsStringKeyword = () =>",
				"    AsStringKeyword.Keyword = \"as_string\"",
				"    AsStringKeyword.Split = (raw) =>",
				"        trimmedRaw = raw.Trim()",
                "        return StringExpression(trimmedRaw)",
				"    return AsStringKeyword",
				"",
				"Keywords.Push(GetAsStringKeyword())",
			});

			dynamic a = d("return as_string Hello, World!");
			
			Assert.That((string) a, Is.EqualTo("Hello, World!"));
		}

		[Test]
		public void Vector2()
		{
			d(new[]
			{
				"Vector2.constructor = (x, y) =>",
				"    this.x = x",
				"    this.y = y",

				"Vector2.+ = (a, b) =>",
				"    return Vector2(a.x + b.x, a.y + b.y)"
			});

			Assert.That((int) d("(Vector2(1, 2) + Vector2(3, 5)).y"), Is.EqualTo(7));
		}

		[Test]
		public void CopyValues()
		{
			d(new[]
			{
				"a = 1",
				"b = a",
				"a = 2"
			});

			var a = d("a");
			var b = d("b");

			Assert.That((int)a, Is.EqualTo(2));
			Assert.That((int)b, Is.EqualTo(1));
		}

		[Test]
		public void NewKeyword()
		{
			{
				d(new[]
				{
					"a = 1",
					"b = new a",
					"c = a",
					"a = 2"
				});

				var a = d("a");
				var b = d("b");
				var c = d("c");

				Assert.That((int) a, Is.EqualTo(2));
				Assert.That((int) b, Is.EqualTo(1));
				Assert.That((int) c, Is.EqualTo(1));
			}
		}
	}
}