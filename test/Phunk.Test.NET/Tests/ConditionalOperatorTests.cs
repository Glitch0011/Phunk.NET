using System;
using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class ConditionalOperatorTests
	{
		[Test]
		public void ConditonalOperator()
		{
			dynamic engine = new Engine(true);

			var res = engine("true ? 1 : 2");

			Assert.That(res == 1);
		}

		[Test]
		public void ComplexConditonalOperator()
		{
			dynamic engine = new Engine(true);

			engine.A = new Func<int>(() => 1);
			engine.B = new Func<int, int>(i => i + 2);
			
			var res = engine("B(false ? 1 : A())");

			Assert.That(res == 3);
		}

        [Test]
        public void Bool()
        {
            dynamic engine = new Engine();

            Assert.That((bool)engine("return 1 == 1"), Is.True);
        }

        [Test]
        public void BoolFalse()
        {
            dynamic engine = new Engine();

            Assert.That((bool)engine("return 1 != 1"), Is.False);
        }

        [Test]
        public void NullCheck()
        {
            dynamic engine = new Engine();

            Assert.That((bool)engine(new[]
            {
                "a = null",
                "return a == null"
            }, Is.True));
        }

        [Test]
        public void NullNotCheck()
        {
            dynamic engine = new Engine();

            Assert.That((bool)engine(new[]
            {
                "a = 1",
                "return a != null"
            }, Is.True));
        }
    }
}