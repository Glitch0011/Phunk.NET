using System.Collections.Generic;
using System.Linq;
using Phunk.Luan.Expressions;

namespace Phunk.Luan.Exceptions
{
    public class ValidOperatorNotFoundException : LuanException
    {
        private IEnumerable<FunctionAbstract> Expressions { get; }

        private string LeftClass { get; }
        private string RightClass { get; }
        private string Operator { get; }

        public ValidOperatorNotFoundException(IEnumerable<FunctionAbstract> expressions, string leftClass, string rightClass,
            string @operator)
        {
            Expressions = expressions;
            LeftClass = leftClass;
            RightClass = rightClass;
            Operator = @operator;
        }

        public override string Message
        {
            get
            {
                var o = string.Join(",",
                    Expressions.Select(
                        x => $"({x.Arguments.First().Type} {Operator} {x.Arguments.Skip(1).FirstOrDefault()?.Type})"));

                return $"Valid operator not found for ({LeftClass} {Operator} {RightClass}), possible operators were: " + o;
            }
        }
	}
}