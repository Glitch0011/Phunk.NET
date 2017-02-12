using System.Collections.Generic;
using Phunk.Luan.Expressions;
using Phunk.Values;

namespace Phunk.Luan.Interfaces
{
    public interface IExpression
	{
	    Value Process(ValueContainer scopes);

        IList<IExpression> Children { get; }

	    IDebugger Debugger { get; set; }
	    CodeLine CodeLine { get; set; }
	}
}