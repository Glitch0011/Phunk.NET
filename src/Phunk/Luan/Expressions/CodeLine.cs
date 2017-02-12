using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions
{
	public class CodeLine
	{
		public string Raw { get; set; }

		public CodeLine Parent { get; set; }

		public int StartPos { get; set; }

		public IExpression Expression { get; set; }
		public string File { get; set; }
		public int LineNumber { get; set; }

		public int Length => Raw.Length;

		public int FullStartPos => (Parent?.FullStartPos ?? 0) + StartPos;

		public CodeLine(string raw)
		{
			Raw = raw;
		}

		public CodeLine(CodeLine parent, string raw, int startPos)
		{
			Raw = raw;
			StartPos = startPos;
			Parent = parent;
		}
        
		public CodeLine Copy()
		{
			return new CodeLine(Raw)
			{
				LineNumber = LineNumber,
				Expression = Expression,
				File = File,
				Parent = Parent,
				StartPos = StartPos
			};
		}

		public CodeLine CreateChild(string raw, int startPos = 0)
		{
			return new CodeLine(this, raw, startPos) {LineNumber = LineNumber};
		}

		public override string ToString()
		{
			return $"{Raw.Trim()}";
		}
	}
}