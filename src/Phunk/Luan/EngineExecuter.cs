using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Phunk.Luan.Expressions;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan
{
	public class EngineExecuter : Executable, IDisposable
	{
		public Engine Engine { get; set; }

        public override Value Globals => Engine.Globals;

        public override ValueContainer GlobalContainer => Engine.GlobalContainer;
        
		private ExpressionBuilder Builder { get; }

		public Action OnDispose { get; set; }

        public override IEngine RootEngine => Engine;

        public EngineExecuter(Engine engine, Value globalOverride = null)
		{
			Engine = engine;
			Builder = new ExpressionBuilder(this);

            OverrideGlobal = globalOverride;
        }
		
		public void Dispose()
		{
			OnDispose();
		}

        public object RunLine(CodeLine codeLine, params Tuple<string, Value>[] args)
        {
            var line = codeLine.Raw;

            if (string.IsNullOrEmpty(line))
                return null;

            line = line.Replace("    ", "\t");

            var tabs = line.ToCharArray().TakeWhile(i => i == '\t').Count();

            if (Builder.TabCount > 0)
            {
                if (tabs == Builder.TabCount)
                {
                    //It's a new line to append
                    var currentChunkExpression = Builder.CurrentExpression as ChunkExpression;
                    if (currentChunkExpression != null)
                    {
                        var codeLineWithoutTabs = codeLine.Copy();

						//codeLineWithoutTabs.Raw = codeLineWithoutTabs.Raw.Replace("    ", "\t");

						var spaceCount = codeLineWithoutTabs.Raw.ToCharArray().TakeWhile(x => x == ' ').Count();
						var tabCount = codeLineWithoutTabs.Raw.ToCharArray().TakeWhile(x => x == '\t').Count();

						codeLineWithoutTabs.StartPos = spaceCount + tabCount;

						codeLineWithoutTabs.Raw = codeLineWithoutTabs.Raw.TrimStart('\t');
						
						currentChunkExpression.Add(Builder.Split(codeLineWithoutTabs));

                        return null;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                else if (tabs < Builder.TabCount)
                {
                    //It's a de-pop
                    Builder.ExpressionStack.Pop();

                    return RunLine(codeLine, args);
                }
                else if (tabs > Builder.TabCount)
                {
                    //It's a.... something?
                    throw new NotSupportedException();
                }
                else
                {
                    //Wha?!
                    throw new NotSupportedException();
                }
            }
            else
            {
                return RunExpression(Builder.Split(codeLine), args);
            }
        }

		public override object RunLines(IEnumerable<CodeLine> code, Tuple<string, Value>[] args = null)
		{
			object ret = null;

			foreach (var line in code)
				ret = RunLine(line, args);

			return ret;
		}
        
		public override Value NewValue(object raw = null, Value @class = null)
		{
			return Engine.NewValue(raw, @class);
		}
	}
}