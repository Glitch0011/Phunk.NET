using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class IfTests
	{
		[Test]
		public void IfTrueSingleLine()
		{
			dynamic engine = new Engine(true);

			Assert.That(engine("if (true) 1") == 1);
		}

		[Test]
		public void IfTrueMultiLine()
		{
			dynamic engine = new Engine(true);

			engine(new[]
			{
				"ifTest = () =>",
				"    if (true)",
				"        return 1",
				"    return 2"
			});

			Assert.That((int) engine("ifTest()"), Is.EqualTo(1));
		}

		[Test]
		public void IfFalseSingleLine()
		{
			dynamic engine = new Engine(true);

			Assert.That(engine("if (false) 1").IsNull);
		}

		[Test]
		public void NotOperatorComplex()
		{
			dynamic engine = new Engine();

			engine("Test = (a) => a");
			engine("a = !Test(true))");
			Assert.That((bool) engine("a"), Is.EqualTo(false));
		}

		[Test]
		public void IfMemberNot()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"test = () =>",
				"    e.b = () =>",
				"    if (e.b != null)",
				"        return 1",
				"    return 2"
			});

			Assert.That((int)engine("test()"), Is.EqualTo(1));
		}

		[Test]
		public void IfMember()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"test = () =>",
				"    e.b = () =>",
				"    if (e.b == null)",
				"        return 1",
				"    return 2"
			});

			Assert.That((int)engine("test()"), Is.EqualTo(2));
		}

		[Test]
		public void If()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"test = () =>",
				"    if (1 == 1)",
				"        return 1",
				"    return 2",
			});

			Assert.That((int) engine("test()"), Is.EqualTo(1));
		}

		[Test]
		public void NotOperator()
		{
			dynamic engine = new Engine();

			engine("a = !true");

			Assert.That((bool) engine("a"), Is.False);
		}

		[Test]
		public void IfNot()
		{
			dynamic engine = new Engine();
			
			engine(new[]
			{
				"test = () =>",
				"    if (1 != 2)",
				"        return 4",
				"    return 3",
			});

			Assert.That((int)engine("test()"), Is.EqualTo(4));
		}
	}
}