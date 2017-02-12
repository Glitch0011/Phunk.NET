using System.Collections.Generic;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
    internal class MemberAccessExpression : Expression
    {
        private IExpression Value { get; }
        private IExpression Member { get; }

        public override IList<IExpression> Children => new[] {Value, Member};

        public MemberAccessExpression(IExpression leftHandSide, IExpression rightHandSide)
        {
            Value = leftHandSide;
            Member = rightHandSide;
        }

        public override string ToString()
        {
            return $"{Value}.{Member}";
        }

        protected override Value Evaluate(ValueContainer scopes)
        {
            var left = Value.Process(scopes);

            if (Member is ValueExpression)
            {
                var lowerContexts = new ValueContainer(scopes.Engine);

                //lowerContexts.Push(scopes.Engine.Globals);

                lowerContexts.NewStackBoundary();

                lowerContexts.Push(left);

                lowerContexts.Stack.AccessingMember = true;

                return Member.Process(lowerContexts);
            }
            else
            {
                var lowerContexts = scopes.Copy();

                //lowerContexts.NewStackBoundary();
                //lowerContexts.Stack.AccessingMember = true;

                lowerContexts.Stack.AccessingMember = false;

                lowerContexts.Push(left);

                return Member.Process(lowerContexts);
            }
        }
    }
}