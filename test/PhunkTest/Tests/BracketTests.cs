using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class BracketTests
	{
		[Test]
		public void SimpleBracket()
		{
			dynamic engine = new Engine();

			Assert.That((int)engine("(1 + 2)"), Is.EqualTo(3));
		}

		[Test]
		public void ComplicatedTest()
		{
			dynamic engine = new Engine();

			engine("a.b = 2");

			Assert.That((int)engine("(a).b"), Is.EqualTo(2));
		}

		[Test]
		public void MemberTest()
		{
			dynamic engine = new Engine();

			engine("Number.Blue = () => return 2");

			Assert.That((int) engine("(1 + 2).Blue()"), Is.EqualTo(2));
		}

		[Test]
		public void MemberTestLambda()
		{
			dynamic engine = new Engine();

			engine("Number.Blue = () => return 2");
			engine("a = () => (1 + 2).Blue()");

			Assert.That((int)engine("a()"), Is.EqualTo(2));
		}
	}
}