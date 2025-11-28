using System.IO;
using System.Collections.Generic;

namespace MauiApp24.Parsers
{
    public interface IParsingStrategy
    {
        List<Software> Parse(Stream stream);
    }
}
