using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MauiApp24.Parsers
{
    public class DOMParsingStrategy : IParsingStrategy
    {
        public List<Software> Parse(Stream stream)
        {
            var result = new List<Software>();
            var doc = new XmlDocument();
            doc.Load(stream);

            foreach (XmlNode node in doc.SelectNodes("//Software"))
            {
                var soft = new Software();

                // Спочатку атрибути
                if (node.Attributes?["Name"] != null)
                    soft.Name = node.Attributes["Name"].Value;
                if (node.Attributes?["Version"] != null)
                    soft.Version = node.Attributes["Version"].Value;

                // Потім елементи (перезаписуємо атрибути, якщо вони були порожні)
                foreach (XmlNode child in node.ChildNodes)
                {
                    switch (child.Name)
                    {
                        case "Name":
                            if (string.IsNullOrEmpty(soft.Name))
                                soft.Name = child.InnerText;
                            break;
                        case "Version":
                            if (string.IsNullOrEmpty(soft.Version))
                                soft.Version = child.InnerText;
                            break;
                        case "Annotation": soft.Annotation = child.InnerText; break;
                        case "Type": soft.Type = child.InnerText; break;
                        case "Author": soft.Author = child.InnerText; break;
                        case "Year": soft.Year = child.InnerText; break;
                        case "Location": soft.Location = child.InnerText; break;
                        case "UsageConditions": soft.UsageConditions = child.InnerText; break;
                    }
                }
                result.Add(soft);
            }
            return result;
        }
    }
}