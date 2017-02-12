using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public static class ContextTests
	{
		[Test]
		public static void DirectReferencesDontPersistOutOfAFunction()
		{
			dynamic e = new Engine(true);

			e(new[]
			{
				"Test = (n) =>",
				"    n = 2",
				"    return n"
			});

			e(new[]
			{
				"run = () =>",
				"    i = 1",
				"    Test(i)",
				"    return i"
			});

			Assert.That((int)e("run()"), Is.EqualTo(1));
		}

		[Test]
		public static void ObjectReferencePersistsIntoAFunction()
		{
			dynamic e = new Engine(true);

			e(new[]
			{
				"Test = (n) =>",
				"    n.test = 2",
				"    return n"
			});

			e(new[]
			{
				"run = () =>",
				"    i.test = 1",
				"    Test(i)",
				"    return i.test"
			});

			Assert.That((int)e("run()"), Is.EqualTo(2));
		}

		[Test]
		public static void AllValuesAreReferences()
		{
			dynamic e = new Engine();

			e(new[]
			{
				"a = Array()",
				"run = () =>",
				"    for (i=0;i<3;i++)",
				"        a.Add(new i)"
			});

			e("run()");

			Assert.That((int[]) e("a"), Is.EquivalentTo(new[] {0, 1, 2}),
				"i is being added as a reference rather than a value");
		}

		[Test]
		public static void DoubleLayer()
		{
			dynamic e = new Engine();

			e(new[]
			{
				"A.constructor = () =>",

				"A.B = () =>",
				"    return 1"
			});

			e("e = A()");

			Assert.That(e("e.B()") == 1);
		}

		[Test]
		public static void LambdaThis()
		{
			dynamic e = new Engine(true);

			e(new[]
			{
				"Obj.constructor = () =>",
				"    this.x = 2",

				"Obj.Test = () =>",
				"    return x",

				"l = (y) => return y.Test()",
			});

		    var l = e("l(Obj())");

            Assert.That((int) l, Is.EqualTo(2));
		}

		[Test]
		public static void PreferThisOverArguments()
		{
			dynamic e = new Engine(true);

			e(new[]
			{
				"Obj.constructor = () =>",
				"    this.x = 2",

				"Obj.Test = (x) =>",
				"    return x",

				"a = Obj()",
			});

			var l = e("a.Test(1)");

			Assert.That((int) l, Is.EqualTo(2));
		}

		[Test]
		public static void Test()
		{
			dynamic e = new Engine(true);

			e(new[]
			{
				"Test.constructor = () =>",
				"    this.a = 1",
				"Test.Hi = (a) =>",
				"    return a",
				"b = Test() "
			});

			Assert.That((int) e("b.Hi(2)"), Is.EqualTo(2));
		}

		[Test]
		public static void TripleLayer()
		{
			dynamic e = new Engine();

			e(new[]
			{
				"A.constructor = () =>",

				"A.B = () =>",
				"    return 2",

				"A.B.C = () =>",
				"    return 1"
			});

			var a = e("e = A()");
			var b = e("e.B()");

			Assert.That(b == 1, "We're executing C rather than B");
		}
	}
}