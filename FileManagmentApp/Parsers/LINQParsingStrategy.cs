using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MauiApp24.Parsers
{
    public class LINQParsingStrategy : IParsingStrategy
    {
        public List<Software> Parse(Stream stream)
        {
            var doc = XDocument.Load(stream);
            var result = new List<Software>();

            foreach (var elem in doc.Descendants("Software"))
            {
                var software = new Software();

                // Спочатку беремо з атрибутів
                software.Name = elem.Attribute("Name")?.Value ?? "";
                software.Version = elem.Attribute("Version")?.Value ?? "";

                // Потім з елементів (перезаписуємо, якщо атрибути були порожні)
                if (string.IsNullOrEmpty(software.Name))
                    software.Name = elem.Element("Name")?.Value ?? "";

                if (string.IsNullOrEmpty(software.Version))
                    software.Version = elem.Element("Version")?.Value ?? "";

                // Інші елементи
                software.Annotation = elem.Element("Annotation")?.Value ?? "";
                software.Type = elem.Element("Type")?.Value ?? "";
                software.Author = elem.Element("Author")?.Value ?? "";
                software.Year = elem.Element("Year")?.Value ?? "";
                software.Location = elem.Element("Location")?.Value ?? "";
                software.UsageConditions = elem.Element("UsageConditions")?.Value ?? "";

                result.Add(software);
            }
            return result;
        }
    }
}