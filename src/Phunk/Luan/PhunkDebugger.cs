using System;
using System.Diagnostics;
using System.Linq;
using Phunk.Luan.Expressions;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan
{
	public class PhunkDebugger : IDebugger
	{
		private int Tab { get; set; }

        public Action<string> ConsoleWrite { get; set; } = s =>
        {
            Console.WriteLine(s);
        };

        //[DebuggerStepThrough]
        void IDebugger.OnPreEvaluateExpression(Expression expression, ValueContainer scopes)
		{
			Tab++;

			var toNotShow = new[]
			{
					typeof(ValueExpression),
					typeof(NumberExpression)
				};

			if (!toNotShow.Contains(expression.GetType()))
				ConsoleWrite($"-> {Enumerable.Range(0, Tab).Select(x => '\t').Rejoin()} {expression.CodeLine}");
		}

        //[DebuggerStepThrough]
        Value IDebugger.OnPostEvaluateExpression(Expression expression, ValueContainer scopes, Value res)
		{
			var r = res?.RawValue?.ToString();

			if (string.IsNullOrEmpty(r))
				r = res?.ToStringLight();

			ConsoleWrite(
				$"<- {Enumerable.Range(0, Tab).Select(x => '\t').Rejoin()} {expression.CodeLine} (= {r})");

			Tab--;

			return res;
		}

		void IDebugger.OnEngineExecutorStart(EngineExecuter e)
		{
			
		}

		void IDebugger.OnEngineExecutorFinish(EngineExecuter e)
		{
			ConsoleWrite("");
		}
	}
}