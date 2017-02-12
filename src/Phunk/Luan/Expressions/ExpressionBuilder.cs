using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Phunk.Luan.Expressions.KeywordExpressions;
using Phunk.Luan.Expressions.Operators;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Text.RegularExpressions;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Phunk.Luan.Expressions
{
    public class RegexReplacements
    {
        public RegexReplacements(dynamic x)
        {
            Regex = x.Regex;
            Replacement = x.Replace;
        }

        public string Regex { get; set; }
        public string Replacement { get; set; }
    }

    internal class ExpressionBuilder
	{
		public Stack<Expression> ExpressionStack { get; } = new Stack<Expression>();
		public int TabCount => ExpressionStack.Count;
		public Expression CurrentExpression => ExpressionStack.First();

		public IList<KeywordExpressionBuilder> DefinedKeywordExpressionBuilders { get; } =
			new List<KeywordExpressionBuilder>();

        public IList<RegexReplacements> Replacements
        {
            get
            {
                if (Engine != null)
                {
                    var r = ((dynamic)Engine.Globals).Regexes;

                    var list = r.IsArray ? (List<Value>)r : null;

                    return list?.Select(x => new RegexReplacements(x)).ToList() ?? new List<RegexReplacements>();
                }
                else
                {
                    return new List<RegexReplacements>();
                }
            }
        }

		public IList<IKeyworkBuilder> KeywordExpressionBuilders
		{
			get
			{
				var a = DefinedKeywordExpressionBuilders.OfType<IKeyworkBuilder>().ToList();

				if (Engine != null)
				{
					var b = ((dynamic) Engine.Globals).Keywords;

					var list = ((Value) b).IsArray ? (List<Value>) b : null;

					var c = list?.Select(x => new InternallyDefinedKeywordBuilder(Engine, x)).OfType<IKeyworkBuilder>().ToList() ??
					                          new List<IKeyworkBuilder>();

					return a.Union(c).ToList();
				}
				else
				{
					return a.ToList();
				}
			}
		}

		private EngineExecuter EngineExecuter { get; set; }

		private Engine Engine => EngineExecuter.Engine;

		public ExpressionBuilder(EngineExecuter engineExecuter)
		{
			EngineExecuter = engineExecuter;
            
            var keywordBuilders =
                typeof(ExpressionBuilder).GetTypeInfo().Assembly
                    .GetTypes()
					.Where(x => typeof(KeywordExpressionBuilder).IsAssignableFrom(x) && !x.GetTypeInfo().IsAbstract)
					.Select(x => Activator.CreateInstance(x, this))
					.OfType<KeywordExpressionBuilder>();

			foreach (var keywork in keywordBuilders)
				DefinedKeywordExpressionBuilders.Add(keywork);
		}

		public Expression GetNewChunk()
		{
			var chunkExpression = new ChunkExpression();

			ExpressionStack.Push(chunkExpression);

			return chunkExpression;
		}

		public IDebugger Debugger => Engine.Debugger;

		public IExpression Split(CodeLine raw)
		{
		    var exp = SplitExpression(raw);

		    if (exp != null)
		    {
		        exp.Debugger = Debugger;

		        exp.CodeLine = raw;

		        raw.Expression = exp;
		    }

		    return exp;
		}

	    private IExpression SplitExpression(CodeLine codeLine)
	    {
	        var raw = codeLine.Raw;

	        if (string.IsNullOrEmpty(raw?.Trim()))
	            return null;

	        raw = raw.TrimEnd(';');
	        raw = raw.TrimStart(' ');

            int iScan = 0;
            if (int.TryParse(raw, out iScan))
                return ParseValue(codeLine);

            double dScan = 0.0;
            if (double.TryParse(raw, out dScan))
                return ParseValue(codeLine);

		    if (raw.Trim().Length == 1)
		    {
			    return ParseValue(codeLine.CreateChild(raw.Trim(), 0));
		    }

		    var leftHandSide = string.Empty;
	        var expressions = new List<Expression>();

            foreach (var replacement in Replacements)
            {
                raw = new Regex(replacement.Regex).Replace(raw, replacement.Replacement);
            }

	        foreach (var keywordBuilder in KeywordExpressionBuilders)
	        {
	            var split = raw.Split(' ').Select(x => x.Trim().ToLowerInvariant()).ToList();

	            if (keywordBuilder.Keyword != null)
	            {
	                if (split.FirstOrDefault() == keywordBuilder.Keyword.Trim().ToLowerInvariant())
	                {
	                   var offset = keywordBuilder.Keyword.Length;

		                return
			                keywordBuilder.Split(codeLine.CreateChild(raw.Skip(offset).Rejoin(), offset));
	                }
	            }
	        }

	        for (var i = 0; i < raw.Length; i++)
	        {
	            var c = raw[i];

	            switch (c)
	            {
	                case '\t':
	                    var chunk = raw.Substring(i + 1);
	                    expressions.Add(new IndentedExpression(Split(new CodeLine(codeLine, chunk, i + 1))));
	                    i += chunk.Length;
	                    break;
	                case '=':
	                {
						//=>
	                    if (raw.Skip(i).Skip(1).Take(1).FirstOrDefault() == '>')
	                    {
	                        var nextChunk = raw.Substring(i + 2);

		                    expressions.Add(new LambdaExpression(leftHandSide,
			                    Split(new CodeLine(codeLine, nextChunk, i + 2)), EngineExecuter));

	                        i += nextChunk.Length + 1;
	                        leftHandSide = string.Empty;
	                    }
	                    else if (string.Join("", raw.Skip(i).Take(2)) == "==")
	                    {
	                        var rightStr = raw.Substring(i + 2, BraceScan(raw.Skip(i)) - 1);
	                        var left = Split(codeLine.CreateChild(leftHandSide, 0)) ?? expressions.Pull();

	                        expressions.Add(new EqualsExpression(left, Split(codeLine.CreateChild(rightStr, i + 2))));
	                        
	                        i += rightStr.Length + 1;
	                        leftHandSide = string.Empty;
	                    }
	                    else
	                    {
	                        var left = Split(codeLine.CreateChild(leftHandSide)) ?? expressions.Pull();
	                        var rightStr = string.Join("", raw.Skip(i).Skip(1));
	                        expressions.Add(new AssignmentExpression(left, Split(codeLine.CreateChild(rightStr, i + 1))));
	                        leftHandSide = string.Empty;
	                        i += rightStr.Length;
	                    }
	                    break;
	                }
	                case '?':
	                {
	                    var payload = raw.Skip(i).Skip(1).Rejoin();
	                    var splitPayload = payload.Split(':');

	                    if (splitPayload.Length != 2)
	                        throw new NotSupportedException();

	                    expressions.Add(
	                        new ConditionalExpression(
	                            Split(codeLine.CreateChild(leftHandSide)),
	                            Split(codeLine.CreateChild(splitPayload.First(), i + 1)),
	                            Split(codeLine.CreateChild(splitPayload.Last(), i + 1 + splitPayload.First().Length + 1))));

	                    i += payload.Length + 1;
	                    leftHandSide = string.Empty;
	                    break;
	                }
	                case '"':
	                {
	                    var nextChunk = raw.Substring(i + 1, QuoteScan(raw.Skip(i)));
	                    
	                    expressions.Add(new StringExpression(nextChunk));

	                    i += nextChunk.Length + 1;
	                    leftHandSide = string.Empty;
	                    break;
	                }
	                case '(':
	                {
	                    var b = BraceScan(raw.Skip(i)) - 1;
	                    if (i + 1 + b > raw.Length || b < 0)
	                        throw new Exception("Brace not closed");
	                    var stringArguments = raw.Substring(i + 1, b);
	                    var n = LambdaScan(raw.Skip(i)); // - 1;

	                    if (n != null)
	                    {
	                        //It's a lambda "Test.Add((a, b) => a + b)"

	                        var rightHandSide = string.Join("", raw.Skip(i + n.Value)).Trim();
	                        var nextLambdaChunk = string.Join("", rightHandSide.Skip(2));

	                        var rightHandSideLambda = Split(codeLine.CreateChild(nextLambdaChunk, i + n.Value + 2));

	                        if (rightHandSideLambda == null)
	                        {
	                            var lambdaExpression = GetNewChunk();

	                            //This is a multi-line lambda
	                            expressions.Add(new LambdaExpression(stringArguments, lambdaExpression, EngineExecuter));
	                        }
	                        else
	                        {
	                            expressions.Add(new LambdaExpression(stringArguments, rightHandSideLambda, EngineExecuter));
	                        }

	                        i += n.Value + rightHandSide.Length + 1;
	                    }
	                    else
	                    {
		                    var arguments =
			                    ArgumentParser.Parse(stringArguments)
				                    .Select(x => Split(codeLine.CreateChild(x.value, i + x.position)))
				                    .ToList();

	                        if (leftHandSide == null)
	                        {
	                            //We have a prior expression, we are probably a member-function call
	                            var prior = expressions.Pull();

	                            if (prior != null)
	                            {
	                                expressions.Add(new FunctionCallExpression(prior, arguments));
	                            }
	                            else
	                            {
	                                throw new NotSupportedException();
	                            }
	                        }
	                        else if (string.IsNullOrEmpty(leftHandSide.Trim()))
	                        {
	                            expressions.Add(new BracketsExpression(Split(codeLine.CreateChild(stringArguments, i + 1))));
	                        }
	                        else
	                        {
	                            var opts = leftHandSide.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

	                            if (opts.Length == 1)
	                            {
		                            var bp = string.Join(" ", opts.Skip(1)) + "(" + stringArguments + ")";

		                            expressions.Add(WrapExpression(codeLine.CreateChild(leftHandSide + bp),
			                            new FunctionCallExpression(leftHandSide, arguments)));
	                            }
	                            else
	                            {
	                                var bp = string.Join(" ", opts.Skip(1)) + "(" + stringArguments + ")";

		                            var arguments2 =
			                            ArgumentParser.Parse(bp)
				                            .Select(x => Split(codeLine.CreateChild(x.value, i + x.position)))
				                            .ToList();

	                                expressions.Add(new FunctionCallExpression(opts.FirstOrDefault(), arguments2));
	                            }
	                        }

	                        i += stringArguments.Length + 1;
	                    }

	                    leftHandSide = string.Empty;

	                    break;
	                }
	                case '!':
	                    if (raw.Skip(i).Take(2).Rejoin() == "!=")
	                    {
	                        var rightStr = raw.Substring(i + 2, BraceScan(raw.Skip(i)) - 1);
	                        var right = Split(codeLine.CreateChild(rightStr, i + 2));
	                        var left = Split(codeLine.CreateChild(leftHandSide)) ?? expressions.Pull();

	                        expressions.Add(new NotEqualsExpression(left, right));
	                        
	                        i += rightStr.Length + 1;
                            leftHandSide = string.Empty;
                        }
	                    else
	                    {
	                        var rightStr = raw.Substring(i + 1, BraceScan(raw.Skip(i)));
	                        var right = Split(codeLine.CreateChild(rightStr, i + 1));

	                        expressions.Add(new NotExpression(right));

	                        i += rightStr.Length + 1;
	                    }
	                    break;
	                case '+':
	                case '-':
	                case '/':
	                case '*':
	                case '>':
	                case '<':
	                {
	                    if (raw.Skip(i).Take(2).Rejoin() == "//")
	                    {
	                        var rest = string.Join("", raw.Skip(i).Skip(2));
	                        expressions.Add(new CommentExpression(rest));
	                        i += rest.Length;
	                        break;
	                    }
	                    else if (raw.Skip(i).Take(2).Rejoin() == "++")
	                    {
		                    var leftHandSize = expressions.Pull() ?? ParseValue(codeLine.CreateChild(leftHandSide));
	                        expressions.Add(new AssignmentExpression(leftHandSize,
	                            new AdditionExpression(leftHandSize, Split(codeLine.CreateChild("1", i)))));
	                        i += 2;
	                        break;
	                    }
	                    else if (raw.Skip(i).Take(2).Rejoin() == "--")
	                    {
	                        var leftHandSize = expressions.Pull() ?? ParseValue(codeLine.CreateChild(leftHandSide));
	                        expressions.Add(new AssignmentExpression(leftHandSize,
	                            new SubractionExpression(leftHandSize, Split(codeLine.CreateChild("1", i)))));
	                        i += 2;
	                        break;
	                    }
	                    else
	                    {
	                        var leftHandSize = expressions.Pull() ?? ParseValue(codeLine.CreateChild(leftHandSide));
	                        var symbol = raw.Skip(i).FirstOrDefault();

		                    if (raw.Skip(i + 1).Take(1).Rejoin() == "=")
		                    {
			                    var nextChunk = raw.Substring(i + 2, NextAth(raw.Skip(i + 2)));
			                    i += nextChunk.Length + 2;

			                    var rightHandSide = Split(codeLine.CreateChild(nextChunk, i + 2));

			                    expressions.Add(new AssignmentExpression(leftHandSize,
				                    OperatorSymbolToExpression(symbol, leftHandSize, rightHandSide)));
		                    }
		                    else
		                    {
			                    var nextChunk = raw.Substring(i + 1, NextAth(raw.Skip(i + 1)));

			                    var rightHandSide = Split(codeLine.CreateChild(nextChunk, i + 1));

			                    i += nextChunk.Length;

			                    Expression outExp = OperatorSymbolToExpression(symbol, leftHandSize, rightHandSide);

			                    if (outExp != null)
				                    expressions.Add(outExp);
		                    }

		                    break;
	                    }
	                }
	                case '.':
	                {
	                    Expression leftHandSize;

	                    if (expressions.Count == 0)
	                    {
	                        leftHandSize = ParseValue(codeLine.CreateChild(leftHandSide));
	                    }
	                    else
	                    {
	                        leftHandSize = expressions.Pull();
	                    }

	                    var nextChar = raw.Skip(i).Skip(1).FirstOrDefault();

	                    IExpression expChunk = null;

	                    //This is an operator
	                    if (new[] {'+', '-', ';', '/', '*'}.Contains(nextChar))
	                    {
	                        expChunk = Split(codeLine.CreateChild(nextChar.ToString(), i));
	                        i += 1;
	                    }
	                    else
	                    {
	                        var nextChunk = raw.Substring(i + 1, NextMember(raw.Skip(i + 1)));
	                        if (!nextChunk.Contains('('))
	                            nextChunk = nextChunk.TrimEnd(')');
	                        expChunk = Split(codeLine.CreateChild(nextChunk, i + 1));
	                        i += nextChunk.Length;
	                    }

	                    if (expChunk != null)
	                    {
	                        expressions.Add(new MemberAccessExpression(leftHandSize, expChunk));
	                        leftHandSide = null;
	                    }
	                    break;
	                }
	                default:
	                    leftHandSide += c;
	                    break;
	            }
	        }

	        return Flush(expressions, codeLine.CreateChild(leftHandSide));
	    }

		private Expression WrapExpression(CodeLine codeLine, FunctionCallExpression exp)
		{
			exp.Debugger = Debugger;
			exp.CodeLine = codeLine;
			codeLine.Expression = exp;

			return exp;
		}

		private static Expression OperatorSymbolToExpression(char symbol, Expression leftHandSize, IExpression rightHandSide)
		{
			Expression outExp;
			switch (symbol)
			{
				case '+':
					outExp = (new AdditionExpression(leftHandSize, rightHandSide));
					break;
				case '-':
					outExp = (new SubractionExpression(leftHandSize, rightHandSide));
					break;
				case '/':
					outExp = (new DivideExpression(leftHandSize, rightHandSide));
					break;
				case '*':
					outExp = (new MultiplicationExpression(leftHandSize, rightHandSide));
					break;
				default:
					outExp = (new OperatorExpression(leftHandSize, rightHandSide, symbol.ToString()));
					break;
			}
			return outExp;
		}

		private static int NextMember(IEnumerable<char> raw)
		{
			return NextChunk(raw, new[] {'+', '-', ';', '/', '*', '=', '.', '!' /*'(', ')'*/});
		}

		private static int NextAth(IEnumerable<char> raw)
		{
			return NextChunk(raw, new[] {'+', '-', ';', '/', '*', '='});
		}

		private static int NextChunk(IEnumerable<char> raw, IList<char> breakExpressions)
		{
			var set = raw.ToList();

			var openBrace = 0;

			for (var i = 0; i < set.Count; i++)
			{
				if (!breakExpressions.Contains('('))
				{
					if (set[i] == '(')
					{
						openBrace++;
					}
					else if (set[i] == ')')
					{
						openBrace--;
					}
				}
				if (openBrace == 0)
				{
					if (breakExpressions.Contains(set[i]))
						return i;
				}
			}

			return set.Count;
		}
		
		private Expression Flush(IList<Expression> expressions, CodeLine codeLine)
		{
			if (expressions.Count == 1)
				return expressions.First();

			if (expressions.Count > 0)
			{
				//System.Diagnostics.Debugger.Break();
				return null;
			}

			var leftHandSide = codeLine.Raw;

			var expression = leftHandSide.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

			if (expression.Length > 1)
			{
				switch (expression.First())
				{
					case "return":
						return new ValueExpression(string.Join(" ", expression.Skip(1)));
					default:
						return ParseValue(codeLine.CreateChild(leftHandSide.Trim()));
				}
			}
			return ParseValue(codeLine.CreateChild(leftHandSide.Trim()));
		}

		private Expression ParseValue(CodeLine codeLine)
		{
			Expression exp = null;

			var isNumber = 0;
            var isDouble = 0.0;

            if (int.TryParse(codeLine.Raw, out isNumber))
            {
                exp = new NumberExpression(codeLine.Raw) { CodeLine = codeLine };
            }
            else if (double.TryParse(codeLine.Raw, out isDouble))
            {
                exp = new NumberExpression(isDouble) { CodeLine = codeLine };
            }
            else
            {
                exp = new ValueExpression(codeLine.Raw) { CodeLine = codeLine };
            }

			exp.Debugger = Debugger;
			exp.CodeLine = codeLine;
			codeLine.Expression = exp;

			return exp;
		}
		
		private static int? LambdaScan(IEnumerable<char> lookahead)
		{
			var set = lookahead.ToList();

			var openBrace = 0;

			for (var i = 0; i < set.Count(); i++)
			{
				switch (set[i])
				{
					case '(':
						openBrace++;
						break;
					case ')':
						openBrace--;
						break;
				}

				if (openBrace == 0)
				{
					if (set.Skip(i).Take(2).SequenceEqual("=>".ToCharArray()))
						return i; // + v;
				}
			}

			return null;
		}

		public static int BraceScan(IEnumerable<char> lookahead, int openBrace = 0)
		{
			var set = lookahead.ToList();

			for (var i = 0; i < set.Count; i++)
			{
				switch (set[i])
				{
					case '(':
						openBrace++;
						break;
					case ')':
						openBrace--;

						if (openBrace == 0)
							return i; //- 1;

						break;
					default:
						continue;
				}
			}

			if (openBrace > 0)
				throw new Exception("Brace not closed");

			return set.Count - 1;
		}

		private static int QuoteScan(IEnumerable<char> lookahead)
		{
			var set = lookahead.Skip(1).ToList();

			for (var i = 0; i < set.Count; i++)
			{
				switch (set[i])
				{
					case '"':
						return i;
					default:
						continue;
				}
			}

			return set.Count;
		}
	}
}