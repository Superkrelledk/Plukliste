namespace Plukliste.Core.Parsers;

/// <summary>
/// Factory for creating appropriate parser based on file type.
/// Follows Factory Pattern and Open/Closed Principle (open for extension, closed for modification)
/// </summary>
public class PluklisteParserFactory
{
    private readonly List<IPluklisteParser> _parsers;

    public PluklisteParserFactory()
    {
        // Register all available parsers
        _parsers = new List<IPluklisteParser>
        {
            new XmlPluklisteParser(),
            new CsvPluklisteParser(),
            new JsonPluklisteParser()
        };
    }

    /// <summary>
    /// Constructor for dependency injection - allows adding custom parsers
    /// </summary>
    public PluklisteParserFactory(IEnumerable<IPluklisteParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    /// <summary>
    /// Gets the appropriate parser for the given file
    /// </summary>
    public IPluklisteParser GetParser(string filePath)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(filePath));
        
        if (parser == null)
        {
            var extension = Path.GetExtension(filePath);
            throw new NotSupportedException($"No parser available for file type: {extension}");
        }

        return parser;
    }

    /// <summary>
    /// Parses a file using the appropriate parser
    /// </summary>
    public Models.IPlukliste ParseFile(string filePath)
    {
        var parser = GetParser(filePath);
        return parser.Parse(filePath);
    }
}
