using System;
using NUnit.Framework;

namespace Phunk.Luan.Tests
{
	public class ClassTests
	{
		[Test]
		public void InheritanceConstructors()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"A.constructor = () =>",
				"    this.Alpha = 1",

				"B.constructor = () =>",
				"    this.Beta = 2",

				"B.SetClass(A)",

				"bravo = B()",
			});

			Assert.That((int) engine("bravo.Alpha"), Is.EqualTo(1));
		}

        [Test]
        public void VariableNameSameAsClassType()
        {
            dynamic engine = new Engine();

            engine(new[]
            {
                "Blue.constructor = () =>",

                "Blue.Test = () =>",
                "    return 2",

                "Red.constructor = () =>",
                "    this.Blue = Blue()",
            });

            Assert.Fail();
        }

        [Test]
		public void CreateClass()
		{
			dynamic engine = new Engine();

			engine("this.Test.constructor = () => this.Blue = true");

			engine("this.Test.Add = (a, b) => a + b");

			var output = engine("a = Test()");

			Assert.That((bool)output.Blue, Is.True);
			Assert.That(output.Add, Is.Not.Null);
		}
		
		[Test]
		public void Vector2()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"Vector2.constructor = (x, y) =>",
				"	this.x = x",
				"	this.y = y"
			});

			engine("Vector2.+ = (a, b) => Vector2(a.x + b.x, a.y + b.y)");

			engine("a = Vector2(1, 2) + Vector2(3, 4)");

			Assert.That(engine("a.x") == 4);
			Assert.That(engine("a.y") == 6);
		}

		[Test]
		public void MultipleTabs()
		{
			dynamic e = new Engine(true);

			e.sys.Square = new Func<int, int>((num) => num*num);

			e(new[]
			{
				"A.constructor= () =>",
				"    this.B = (num) =>",
				"        v = sys.Square(num)",
				"        return v",
				"    this.C = 2"
			});

			e("c = A()");
			e("out = c.B(4)");

			Assert.That(e("out") == 16);
		}

		[Test]
		public void MultipleConstructorsTabbedVector2()
		{
			dynamic engine = new Engine();

			engine(new[]
			{
				"Vector2.constructor = () =>",
				"	this.x = 0",
				"	this.y = 0",

				"Vector2.constructor = (x, y) =>",
				"	this.x = x",
				"	this.y = y",

				"Vector2.+ = (a, b) =>",
				"	c = Vector2()",
				"	c.x = a.x + b.x;",
				"	c.y = a.y + b.y)",
				"	return c"
			});

			engine("a = Vector2(1, 2) + Vector2(3, 4)");

			Assert.That(engine("a.x") == 4);
			Assert.That(engine("a.y") == 6);
		}
	}
}