using System;
using NUnit.Framework;
using Phunk.Values;

namespace Phunk.Luan.Tests
{
	public class ExpressionTests
	{
		[Test]
		public static void Context()
		{
			dynamic engine = new Engine();

			engine.Hello = new Func<int, int>((i) => 50*i);
			engine.Test.Hello = new Func<int, int>((i) => 10*i);

			Assert.That((int)engine("Test.Hello(2)"), Is.EqualTo(20));
		}

		[Test]
		public static void AlternativeContext()
		{
			dynamic engine = new Engine();
			
			engine.Test.Hello = new Func<int, int>((i) => 10 * i);
			engine.Hello = new Func<int, int>((i) => 50 * i);

			Assert.That((int)engine("Test.Hello(2)"), Is.EqualTo(20));
		}

		[Test]
		public static void StringChainTest()
		{
			dynamic engine = new Engine(true);

			engine.sys.ToUp = new Func<string, string>((str) => str.ToUpper());
			engine.sys.ToDown = new Func<string, string>((str) => str.ToLower());
			engine.sys.ToFirst = new Func<string, string>((str) => str.Substring(0, 1));

			engine("String.ToUp = () => sys.ToUp(this)");
			engine("String.ToDown = () => sys.ToDown(this)");
			engine("String.ToFirst = () => sys.ToFirst(this)");

			Assert.That((string) engine("\"test\".ToUp().ToDown().ToFirst()") == "t");
		}

		[Test]
		public static void ChainTest()
		{
			dynamic engine = new Engine();

			var called = false;

			engine.Add = new Func<int, int, Value>((a, b) =>
			{
				dynamic child = ((Engine)engine).NewValue();

				child.Result = a + b;

				child.Boo = new Action(() =>
				{
					Assert.That((int) child.Result, Is.EqualTo(30));
					called = true;
					//No return value
				});

				return child;
			});

			engine("v = Add(10, 20).Boo()");

			Assert.That(called, Is.True);
		}

		[Test]
		public static void ReturnStringTest()
		{
			dynamic engine = new Engine();
			Assert.That((string) engine("return \"Hi\";"), Is.EqualTo("Hi"));
		}

		[Test]
		public static void ReturnBooleanTest()
		{
			dynamic engine = new Engine();
			Assert.That((bool) engine("return true"), Is.EqualTo(true));
		}
	}
}