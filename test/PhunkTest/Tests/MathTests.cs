using System;
using NUnit.Framework;
using Phunk.Luan.Exceptions;
using Phunk.Values;

namespace Phunk.Luan.Tests
{
	public static class MathTests
	{
		[Test]
		public static void Addition()
		{
			dynamic engine = new Engine();

			Assert.That(engine("1 + 2") == 3);
		}

		[Test]
		public static void Negative()
		{
			var res = ((dynamic) new Engine())("-1");
			Assert.That((int) res, Is.EqualTo(-1));
		}

		[Test]
		public static void IntegerPlusStringDoesNotExist()
		{
			dynamic engine = new Engine();

			Assert.Throws<ValidOperatorNotFoundException>(() => engine("1 + \"a\""));
		}

		[Test]
		public static void IntegerPlusIntegerDoesNotExist()
		{
			dynamic engine = new Engine(true);

			Assert.Throws<OperatorNotFoundException>(() => engine("1 + 2"));
		}

		[Test]
		public static void AddExistsButNotForThisType()
		{
			dynamic engine = new Engine(true);

			engine.Fail = new Action(Assert.Fail);

			engine("Number.+ = (Number a, String b) => Fail()");

			Assert.Throws<ValidOperatorNotFoundException>(() => engine("1 + 3"));
		}

		[Test]
		public static void AdditionWithFunctions()
		{
			dynamic engine = new Engine();

			engine("a = () => 1 + 2");
			engine("b = () => 2 + 3");

			Assert.That(engine("a() + b()") == 8);
		}

		[Test]
		public static void BracketAddition()
		{
			dynamic engine = new Engine();

			Assert.That(engine("(1 + 2) + 1") == 4);
		}

		[Test]
		public static void BracketSubtraction()
		{
			dynamic engine = new Engine();

			Assert.That(engine("(1 - 2) + 4") == 3);
		}

		[Test]
		public static void CorrectSubtractionBrackets()
		{
			dynamic engine = new Engine();

			Assert.That(engine("4 + (1 - 2)") == 3);
		}

		[Test]
		public static void Subtraction()
		{
			dynamic engine = new Engine();

			Assert.That((int) engine("1 - 2"), Is.EqualTo(-1));
		}

		[Test]
		public static void Divide()
		{
			dynamic engine = new Engine();

			Assert.That((int) engine("4 / 2"), Is.EqualTo(2));
		}

		[Test]
		public static void Multiply()
		{
			dynamic engine = new Engine();

			Assert.That((int) engine("3 * 2"), Is.EqualTo(6));
		}

		[Test]
		public static void OverridenAddition()
		{
			dynamic engine = new Engine();

			engine("Number.+=(Number a, Number b)=> sys.Add(a, b)*2");

			Assert.That((int) (engine("1 + 2") as Value), Is.EqualTo(6));
		}

		[Test]
		public static void OverridenAdditionWithSpaces()
		{
			dynamic engine = new Engine();

			engine("Number.+ = (Number a, Number b) => sys.Add(a, b) * 2");

			Assert.That((int) (engine("1 + 2") as Value), Is.EqualTo(6));
		}

		[Test]
		public static void Increment()
		{
			Assert.That((int)((dynamic)new Engine())("(a = 2)++"), Is.EqualTo(3));
		}

		[Test]
		public static void Decrement()
		{
			Assert.That((int)((dynamic)new Engine())("(a = 2)--"), Is.EqualTo(1));
		}

		[Test]
		public static void AutoIncrement()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"a = 1",
				"a += 2",
			});

			Assert.That((int) engine("a"), Is.EqualTo(3));
		}

		[Test]
		public static void AutoMultiply()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"a = 3",
				"a *= 2",
			});

			Assert.That((int)engine("a"), Is.EqualTo(6));
		}

		[Test]
		public static void SqrtInt()
		{
			dynamic engine = new Engine();

			Assert.That((int) engine("Math.Sqrt(4)"), Is.EqualTo(2));
		}

		[Test]
		public static void SqrtDouble()
		{
			dynamic engine = new Engine();

			Assert.That((double)engine("Math.Sqrt(5)"), Is.EqualTo(2.2360679774997898d));
		}
	}
}