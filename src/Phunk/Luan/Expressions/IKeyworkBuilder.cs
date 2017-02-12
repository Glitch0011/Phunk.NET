using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions
{
    internal interface IKeyworkBuilder
	{
		string Keyword { get; }
		IExpression Split(CodeLine raw);
        bool PreEvaluate { get; }
	}
}