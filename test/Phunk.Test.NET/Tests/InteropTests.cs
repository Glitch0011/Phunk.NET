using System;
using System.Collections.Generic;
using NUnit.Framework;
using Phunk.Values;

namespace Phunk.Luan.Tests
{
	public class InteropTests
	{
		[Test]
		public static void NullReturn()
		{
			dynamic engine = new Engine();

			var called = false;

			engine.Add = new Action(() => { called = true; });

			var v = engine("Add()");

			Assert.That(v, Is.Null);
			Assert.That(called, Is.True);
		}

		[Test]
		public static void IntReturn()
		{
			dynamic engine = new Engine();
			
			Assert.That((int)engine("1"), Is.EqualTo(1));
		}

		[Test]
		public static void BooleanReturnTrue()
		{
			dynamic engine = new Engine();

			Assert.That((bool)engine("true"), Is.EqualTo(true));
		}

		[Test]
		public static void BooleanReturnFalse()
		{
			dynamic engine = new Engine();

			Assert.That((bool)engine("false"), Is.EqualTo(false));
		}

		[Test]
		public static void CallWithArguments()
		{
			dynamic engine = new Engine();

			Assert.That(engine("e", new Arg("e", new Value(1))) == 1);
		}

		[Test]
		public static void MultiParameterCall()
		{
			dynamic engine = new Engine();

			engine.Add = new Func<int, int, int>((a, b) => a + b);

			Assert.That((int) engine("return Add(1, 2)"), Is.EqualTo(3));
		}

		[Test]
		public static void SingleParameterCall()
		{
			dynamic engine = new Engine();

			engine.Add = new Func<int, int>(a => a + 2);

			Assert.That((int) engine("return Add(1)"), Is.EqualTo(3));
		}
		
		[Test]
		public static void ObjectWithFunctionReturn()
		{
			dynamic engine = new Engine();

			engine.GetObject = new Func<Value>(() =>
			{
				dynamic child = new Value();

				child.GetResult = new Func<int>(() => 10);

				return child;
			});

			Assert.That((int) engine("GetObject().GetResult()"), Is.EqualTo(10));
		}

		[Test]
		public static void ObjectReturn()
		{
			dynamic engine = new Engine();

			engine.GetObject = new Func<Value>(() =>
			{
				dynamic child = new Value();

				child.Result = 10;

				return child;
			});

			Assert.That((int) engine("GetObject().Result"), Is.EqualTo(10));
		}

		[Test]
		public static void ArrayReturnInt()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"a = Array()",
				"run = () =>",
				"    for (i=0;i<3;i++)",
				"        a.Add(i + 0)"
			});

			engine("run()");
			
			Assert.That((int[])engine("a"), Is.EquivalentTo(new[] {0, 1, 2}));
		}

		[Test]
		public static void ListReturnInt()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"a = Array()",
				"run = () =>",
				"    for (i=0;i<3;i++)",
				"        a.Add(i + 0)"
			});

			engine("run()");

			var expected = new List<int>{0, 1, 2};

			Assert.That((List<int>) engine("a"), Is.EquivalentTo(expected));
		}

		[Test]
		public static void ListReturnString()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"a = Array()",
				"run = () =>",
				"    for (i=0;i<3;i++)",
				"        a.Add(\"test \" + i)",
			});

			engine("run()");

			var expected = new List<string> {"test 0", "test 1", "test 2"};

			Assert.That((List<string>) engine("a"), Is.EquivalentTo(expected));
		}

		[Test]
		public static void MultipleOperators()
		{
			dynamic engine = new Engine();

			engine("sys.ToString = argA",
				new Tuple<string, Value>("argA", new Value(new Func<string, string>(i => i.ToString()))));

			engine("sys.ToString = argA",
				new Tuple<string, Value>("argA", new Value(new Func<int, string>(i => i.ToString()))));

			var a = engine("sys.ToString(1)");
			var b = engine("sys.ToString(\"Hi\")");

			Assert.That(a, Is.Not.Null);
			Assert.That(b, Is.Not.Null);
		}
	}
}