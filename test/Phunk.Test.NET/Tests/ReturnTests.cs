using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class ReturnTests
	{
		[Test]
		public void ReturnTest()
		{
			dynamic engine = new Engine(true);

			engine(new[]
			{
				"A = () =>",
				"    b = 1",
				"    return b"
			});

			Assert.That(engine("A()") == 1);
		}

		[Test]
		public void EarlyReturnTest()
		{
			dynamic engine = new Engine(true);

			engine(new[]
			{
				"A = () =>",
				"    b = 1",
				"    return b",
				"    b = 2"
			});

			Assert.That(engine("A()") == 1);
		}
	}
}