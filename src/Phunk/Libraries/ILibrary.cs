namespace Phunk.Libraries
{
	public interface ILibrary
	{
		string[] Code { get; }
		void RawFunctions(dynamic engine);
	}
}