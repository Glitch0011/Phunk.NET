using NUnit.Framework;
using Phunk.Luan.Exceptions;

namespace Phunk.Luan.Tests
{
	public class ExecutorTests
	{
        [Test]
        public static void CallFunctionWithNullArgument()
        {
            dynamic d = new Engine();

            d(new[]
            {
                "Add = (a) =>",
				"    if (a == null)",
				"        return 3",
				"    return a + 1",
                "a = 1",
            });

            Assert.That((int)d("Add(b)"), Is.EqualTo(3));
        }

		[Test]
		public static void PrivateClass()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
				host(new[]
				{
					"SecretFactory.constructor = () =>",
					"SecretFactory.MakeSecret = () =>",
					"    return 1234"
				});

				host(new[]
				{
					"no_read SecretFactory"
				});

				//Prove secret function is readable from this executor
				Assert.That((int) host(new[]
				{
					"a = SecretFactory()",
					"return a.MakeSecret()"
				}), Is.EqualTo(1234));

				using (var client = d.GetExecuter())
				{
					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("b = SecretFactory()");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("b = SecretFactory.MakeSecret()");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client(new[]
						{
							"SecretFactory.constructor = (a) =>",
							"    return 5"
						});
					});
				}
			}
		}

        [Test]
        public static void RegexLine()
        {
            dynamic d = new Engine();
            
            d(new[]
            {
                "GetAddRegex = () =>",
                "    AddRegex.Regex = \"add (?<A>.*) (?<B>.*)\"",
                "    AddRegex.Replace = \"Add(${A}, ${B})\"",
                "    return AddRegex",

                "Regexes.Push(GetAddRegex())",
            });

            d(new[]
            {
                "Add = (a, b) =>",
                "    return a + b"
            });

            Assert.That((int)d("add 1 2"), Is.EqualTo(3));
        }

        [Test]
        public static void OverridenExecutorContext()
        {
            dynamic d = new Engine();

            var a = d(new[]
            {
                "Container.constructor = () =>",
                "a = Container()",
                "a.First = 1",
                "return a"
            });

            using (var e = d.GetExecuter(a))
            {
                e(new[]
                {
                    "Second = 2",
                });
            }

            Assert.That((int)d("a.Second"), Is.EqualTo(2));
        }

		[Test]
		public static void PrivateMember()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
                host(new[]
                {
                    "SecretFactory.constructor = () =>",
                    "    this.Secret = 1234",
                    "    no_read this.Secret",
                });

                host(new[] {
                    "SecretFactory.GetSecret = () =>",
                    "    return this.Secret",
                });

                host(new[]
                {
                    "factory = SecretFactory()",
                });
                
                Assert.That((int) host(new[]
				{
                    "factory.Secret"
                }), Is.EqualTo(1234));

                using (var client = d.GetExecuter())
				{
					//Try access the secret directly
					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("secret = factory.Secret");
					});

					//Use the allowed function, which by calling should context-swap into the engine-executor with the permission to read secret
					Assert.That((int) client("actual_secret = factory.GetSecret()"), Is.EqualTo(1234));
				}
			}
		}

		[Test]
		public static void PassPrivateClass()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
				host(new[]
				{
					"PValue.constructor = (i) =>",
					"    this.val = i",
				});

				host(new[]
				{
					"no_read PValue"
				});

				//Prove secret function is readable from this executor
				var two = host(new[]
				{
					"return PValue(2)",
				});

				using (var client = d.GetExecuter())
				{
					client("b = SecretFactory()");

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("b = SecretFactory.MakeSecret()");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client(new[]
						{
							"SecretFactory.constructor = (a) =>",
							"    return 5"
						});
					});
				}
			}
		}

		[Test]
		public static void PrivateFunction()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
				host(new[]
				{
					"GetSecret = () =>",
					"    return 1234"
				});
				
                host(new[]
				{
					"no_read GetSecret"
				});

				//Prove secret function is readable from this executor
				Assert.That((int) host("GetSecret()"), Is.EqualTo(1234));
				
				using (var client = d.GetExecuter())
				{
					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("GetSecret()");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("GetSecret() = 0");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("a = GetSecret");
					});
				}
			}
		}

		[Test]
		public static void PrivateVariable()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
				host(new[]
				{
					"Secret = 1"
				});

				host(new[]
				{
					"no_read Secret"
				});

				//Prove secret function is readable from this executor
				Assert.That((int)host("Secret"), Is.EqualTo(1));

				using (var client = d.GetExecuter())
				{
					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("Secret");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("Secret = 1");
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client("a = Secret");
					});
				}
			}
		}

		[Test]
		public static void Class()
		{
			dynamic d = new Engine();

			using (var host = d.GetExecuter())
			{
				host(new[]
				{
					"pool = Array()",

					"Mana.SetName = (str) =>",
					"    this.Name = str",

					"Mana.constructor = (str) =>",
					"    this.SetName(str)",

					"no_read Mana",

					"pool += Mana(\"Blue\")"
				});

				using (var client = d.GetExecuter())
				{
					var output = client(new[]
					{
						"m = pool.Take(1).First()",
						"m.SetName(\"Red\")",
					});

					Assert.Throws<NoReadPermissionException>(() =>
					{
						client(new[]
						{
							"cheat = Mana(\"Green\")",
						});
					});
				}
			}
		}

	}
}