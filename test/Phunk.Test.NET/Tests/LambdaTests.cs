using System;
using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class LambdaTests
	{
		[Test]
		public static void LambdaInlineTest()
		{
			dynamic engine = new Engine();
			var called = false;

			engine.Shout = new Action<string>(str =>
			{
				Assert.That(str, Is.EqualTo("hi"));
				called = true;
			});

			engine(new[]
			{
				"this.Until = (fun) => fun()",
				"this.Until((b) => Shout(\"hi\"))"
			});

			Assert.That(called, Is.True);
		}

		[Test]
		public static void LambdaTest()
		{
			dynamic engine = new Engine();

			var called = false;

			engine.Shout = new Action<string>(str =>
			{
				Assert.That(str, Is.EqualTo("Hello, World"));
				called = true;
			});

			engine(new[]
			{
				"this.Echo = (str) => Shout(str)",
				"Echo(\"Hello, World\")"
			});

			Assert.That(called, Is.True);
		}

		[Test(Description = "A Lambda can be stored as a variable, referenced and still have a correct 'this' context."), Description("Test")]
		public static void LambdaStoredAsAVariable()
		{
			dynamic engine = new Engine(true);

			engine.Square = new Func<int, int>(i => i * i);

			engine("a = () => Square(this.b)");

			engine(new[]
			{
				"ObjA.constructor = () =>",
				"    this.A = a",
				"    this.b = 2"
			});

			engine(new[]
			{
				"ObjB.constructor = () =>",
				"    this.A = a",
				"    this.b = 4"
			});

			engine("c = ObjA()");
			engine("d = ObjB()");
			
			Assert.That(engine("c.A()") == 4);
			Assert.That(engine("d.A()") == 16);
		}


		[Test]
		public static void LambdaWithReturn()
		{
			dynamic engine = new Engine();

			engine("add = (a, b) => (return (a + b));");

			Assert.That((int) engine("return Add(1, 2)"), Is.EqualTo(3));
		}

		[Test]
		public static void LambdaWithoutReturn()
		{
			dynamic engine = new Engine();

			engine("add = (a, b) => (a + b);");

			Assert.That((int) engine("return Add(1, 2)"), Is.EqualTo(3));
		}

		[Test]
		public static void LambdaWithoutReturnOrBrackets()
		{
			dynamic engine = new Engine();

			engine("add = (a, b) => a + b");

			Assert.That((int) engine("return Add(1, 2)"), Is.EqualTo(3));
		}

		[TestCase("Hello, World!")]
		public static void LambdaNoBracketsTest(string message)
		{
			dynamic engine = new Engine();

			var called = false;

			engine.Shout = new Action<string>(str =>
			{
				Assert.That(str, Is.EqualTo(message));
				called = true;
			});

			engine("this.Echo = str => Shout(str)");
			engine($"Echo(\"{message}\")");

			Assert.That(called, Is.True);
		}

		[Test]
		public static void LambdaNoBracketMultipleTest()
		{
			dynamic engine = new Engine();

			engine("Add = a, b => a + b");

			Assert.That((int) engine("Add(1, 2)"), Is.EqualTo(3));
		}

		[Test]
		public static void LambdaNoBracketNoSpacesMultipleTest()
		{
			dynamic engine = new Engine();

			engine("Add=a,b=>a+b");

			Assert.That((int) engine("Add(1,2)"), Is.EqualTo(3));
		}

		[Test]
		public static void OverloadedLambdaTest()
		{
			dynamic engine = new Engine();

			engine("Square = (i) => i * i");
			engine("Square = (a, b) => a * b");

			Assert.That(engine("Square(2)") == 4);
			Assert.That(engine("Square(2, 4)") == 8);
		}

		[Test]
		public static void OverloadedLambdaDifferentTypeTest()
		{
			dynamic engine = new Engine();

			engine("Square = (i) => i * i");
			engine.Square = new Func<int, int, int>((a, b) => a*b);

			Assert.That(engine("Square(2)") == 4);
			Assert.That(engine("Square(2, 4)") == 8);
		}
	}
}