using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions
{
	public interface IKeyworkBuilder
	{
		string Keyword { get; }
		IExpression Split(CodeLine raw);
        bool PreEvaluate { get; }
	}
}