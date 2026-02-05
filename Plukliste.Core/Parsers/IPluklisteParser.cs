namespace Plukliste.Core.Parsers;

public interface IPluklisteParser
{
    /// <summary>
    /// Checks if this parser can handle the given file based on extension or content
    /// </summary>
    bool CanParse(string filePath);
    
    /// <summary>
    /// Parses the file and returns a Plukliste object
    /// </summary>
    Models.IPlukliste Parse(string filePath);
}
