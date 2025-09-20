namespace Jmes.NET.Parsing.Syntax;

public sealed class AstHashMap : AstNode
{
	public required List<(string Key, AstNode Value)> Entries { get; init; }
}
