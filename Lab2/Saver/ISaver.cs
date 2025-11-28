using MauiApp24.Parsers;
using System.Collections.Generic;

namespace Saver
{
    public interface ISaver
    {
        string GenerateContent(List<Software> data);
    }
}
