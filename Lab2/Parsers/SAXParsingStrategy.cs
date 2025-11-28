using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MauiApp24.Parsers
{
    public class SAXParsingStrategy : IParsingStrategy
    {
        public List<Software> Parse(Stream stream)
        {
            var result = new List<Software>();
            Software currentSoftware = null;

            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "Software")
                    {
                        currentSoftware = new Software();
                        // Проблема: назва береться з атрибута, але в XML може бути як елемент
                        currentSoftware.Name = reader.GetAttribute("Name");
                        currentSoftware.Version = reader.GetAttribute("Version");
                    }
                    else if (currentSoftware != null && reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "Name": // Додаємо обробку елемента Name
                                currentSoftware.Name = reader.ReadElementContentAsString();
                                break;
                            case "Annotation":
                                currentSoftware.Annotation = reader.ReadElementContentAsString();
                                break;
                            case "Type":
                                currentSoftware.Type = reader.ReadElementContentAsString();
                                break;
                            case "Author":
                                currentSoftware.Author = reader.ReadElementContentAsString();
                                break;
                            case "Year":
                                currentSoftware.Year = reader.ReadElementContentAsString();
                                break;
                            case "Location":
                                currentSoftware.Location = reader.ReadElementContentAsString();
                                break;
                            case "UsageConditions":
                                currentSoftware.UsageConditions = reader.ReadElementContentAsString();
                                break;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Software")
                    {
                        result.Add(currentSoftware);
                        currentSoftware = null;
                    }
                }
            }
            return result;
        }
    }
}