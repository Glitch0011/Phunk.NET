using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
    internal class InternallyDefinedKeywordBuilder : IKeyworkBuilder
	{
		public dynamic Value { get; set; }

		public dynamic Engine { get; set; }

		public string Keyword => Value.Keyword;

        public bool PreEvaluate
        {
            get
            {
                return (Value.PreEvaluate as Value)?.IsTrue ?? false;
            }
        }

        public InternallyDefinedKeywordBuilder(Engine engine, Value value)
		{
			Engine = engine;
			Value = value;
		}

		public IExpression Split(CodeLine raw)
		{
			var e = Engine(Value.Split, raw);

			if (e is Value)
				return new InternallyDefinedExpression(Engine, e);
			else
				return e;
		}
	}
}