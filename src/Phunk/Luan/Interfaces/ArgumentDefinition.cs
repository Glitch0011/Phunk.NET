namespace Phunk.Luan.Interfaces
{
    internal class ArgumentDefinition
	{
		public string Name { get; }
		public string Type { get; }

		public ArgumentDefinition(string type, string name)
		{
			Type = type;
			Name = name;
		}

		private string TrimmedName => Name.Trim();

		public override string ToString()
		{
			if (Type != null)
				return $"{Type.Trim()} {TrimmedName}";
			return $"{TrimmedName}";
		}
	}
}