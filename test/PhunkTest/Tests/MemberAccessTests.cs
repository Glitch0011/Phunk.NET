using System;
using NUnit.Framework;
using Phunk.Values;

namespace Phunk.Luan.Tests
{
	public class MemberAccessTests
	{
		[Test]
		public static void StringExtensions()
		{
			dynamic engine = new Engine(true);

			engine.sys.ToUpper = new Func<string, string>(obj => obj.ToUpper());

			engine("String.ToUpper = () => sys.ToUpper(this)");

			Assert.That((string)engine("\"Hello\".ToUpper()"), Is.EqualTo("HELLO"));
		}

		[Test]
		public void SimpleOverride()
		{
			dynamic engine = new Engine(true);

			engine("B = 2");
			engine("A.B = 1");

			Assert.That(engine("B") == 2);
		}

		[Test]
		public void StackBoundaryTest()
		{
			dynamic engine = new Engine(true);

			engine.Check = new Action<int>((i) => Assert.That(i == 2));

			engine(new[]
			{
				"A = () =>",
				"    B = 2",
				"    Check(B)",
				"B = 1",
				"A()"
			});

			Assert.That(engine("B") == 1);
		}

		[Test]
		public static void FunctionInMember()
		{
			dynamic engine = new Engine(true);

			engine.a.B = new Func<Value>(() => {
				dynamic dyn = new Value();

				dyn.c = 1;

				return dyn;
			});

			Assert.That(engine("a.B().c") == 1);
		}

		[Test]
		public static void SimpleValue()
		{
			dynamic engine = new Engine();

			engine("a.left = 1");

			Assert.That(engine("a.left") == 1);
		}

		[Test]
		public static void SimpleValueAndSubValue()
		{
			dynamic engine = new Engine();

			engine("a.left = 1");
			engine("a.left.blue = 2");

			Assert.That(engine("a.left") == 1);
			Assert.That(engine("a.left.blue") == 2);
		}

		[Test]
		public static void LambdaAssignment()
		{
			dynamic engine = new Engine();

			engine("a.left = () => 1");

			Assert.That(engine("a.left()") == 1);
		}

		[Test]
		public static void ContextLimitedData()
		{
			dynamic engine = new Engine();

			engine("_age = 1");
			engine("Blue._age = 2");

			Assert.That((int) engine("_age"), Is.EqualTo(1));
		}

		[Test]
		public static void ShouldNotResolveNonExistentMemberAccessWithGlobal()
		{
			dynamic engine = new Engine();

			engine("Age = 1");

			Assert.That(engine("Green.Age").IsNull);
		}
		
		[Test]
		public static void LambdaThis()
		{
			dynamic engine = new Engine(true);

			engine("Obj._age = 1");
			engine("Obj.FakeAge = () => blue._age");
			engine("Obj.RealAge = () => this._age");

			Assert.That(engine("Obj.FakeAge()").IsNull);
			Assert.That(engine("Obj.RealAge()") == 1);
		}

		[Test]
		public static void LeftHandSideReassignemnt()
		{
			dynamic engine = new Engine();

			engine("right = 1");
			engine("left = () => right");
			engine("left() = 2");

			Assert.That(engine("right") == 2);
		}

		[Test]
		public static void LeftHandSideReassignemntThisVal()
		{
			dynamic engine = new Engine();

			engine("right = 1");
			engine("left = () => right");
			engine("left() = 2");

			Assert.That(engine("right") == 2);
		}

		[Test]
		public static void CorrectMember()
		{
			dynamic engine = new Engine();

			engine("a.right = 1");
			engine("b.right = 2");

			Assert.That(engine("a.right") == 1);
		}

		[Test]
		public static void DoesNotLookDownScope()
		{
			dynamic engine = new Engine();

			engine("a.right = 1");
			engine("b.right = 2");

			Assert.That(engine("right").IsNull);
		}

		[Test]
		public void ConstructorThisValue()
		{
			dynamic engine = new Engine();

			engine("Example.constructor = (e) => this.u = e;");

			engine("obj = Example(1)");
			
			Assert.That(engine("obj.u") == 1);
		}

		[Test]
		public void ConstructorThisValueSameName()
		{
			dynamic engine = new Engine();

			engine("Example.constructor = (e) => this.e = e;");

			engine("obj = Example(1)");

			Assert.That(engine("obj.e") == 1);
		}
	}
}