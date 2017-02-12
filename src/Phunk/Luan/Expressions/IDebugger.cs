using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public interface IDebugger
	{
		void OnPreEvaluateExpression(Expression e, ValueContainer scopes);
		Value OnPostEvaluateExpression(Expression e, ValueContainer scopes, Value a);
		void OnEngineExecutorStart(EngineExecuter e);
		void OnEngineExecutorFinish(EngineExecuter e);
	}
}