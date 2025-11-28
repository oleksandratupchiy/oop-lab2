using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using MauiApp24.Parsers;

namespace Saver
{
    public class XmlSaver : ISaver
    {
        public string GenerateContent(List<Software> data)
        {
            try
            {
                // Налаштування для коректного UTF-8
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), // UTF-8 з BOM
                    OmitXmlDeclaration = false,
                    IndentChars = "  ",
                    NewLineChars = "\n"
                };

                using var stringWriter = new StringWriterWithEncoding(Encoding.UTF8);
                using var writer = XmlWriter.Create(stringWriter, settings);

                writer.WriteStartDocument(); // Це додасть <?xml version="1.0"?>
                writer.WriteStartElement("ArrayOfSoftware");

                foreach (var software in data)
                {
                    writer.WriteStartElement("Software");

                    WriteElement(writer, "Name", software.Name);
                    WriteElement(writer, "Version", software.Version);
                    WriteElement(writer, "Annotation", software.Annotation);
                    WriteElement(writer, "Type", software.Type);
                    WriteElement(writer, "Author", software.Author);
                    WriteElement(writer, "Year", software.Year);
                    WriteElement(writer, "Location", software.Location);
                    WriteElement(writer, "UsageConditions", software.UsageConditions);

                    writer.WriteEndElement(); // Software
                }

                writer.WriteEndElement(); // ArrayOfSoftware
                writer.WriteEndDocument();
                writer.Flush();

                string result = stringWriter.ToString();

                // Діагностика
                System.Diagnostics.Debug.WriteLine($"=== XML RESULT ===");
                System.Diagnostics.Debug.WriteLine($"Length: {result.Length}");
                System.Diagnostics.Debug.WriteLine($"First 200 chars: {result.Substring(0, Math.Min(200, result.Length))}");
                System.Diagnostics.Debug.WriteLine($"Contains UTF-8 BOM: {result.StartsWith("\uFEFF")}");

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"💥 XML Generation Error: {ex}");
                return GenerateSimpleUtf8Xml(data);
            }
        }

        private void WriteElement(XmlWriter writer, string elementName, string value)
        {
            writer.WriteStartElement(elementName);
            writer.WriteString(value ?? "");
            writer.WriteEndElement();
        }

        // Резервний метод для генерації простого UTF-8 XML
        private string GenerateSimpleUtf8Xml(List<Software> data)
        {
            var xml = new StringBuilder();
            // Додаємо BOM вручну
            xml.Append('\uFEFF'); // UTF-8 BOM
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<ArrayOfSoftware>");

            foreach (var item in data)
            {
                xml.AppendLine("  <Software>");
                xml.AppendLine($"    <Name>{EscapeXml(item.Name)}</Name>");
                xml.AppendLine($"    <Version>{EscapeXml(item.Version)}</Version>");
                xml.AppendLine($"    <Annotation>{EscapeXml(item.Annotation)}</Annotation>");
                xml.AppendLine($"    <Type>{EscapeXml(item.Type)}</Type>");
                xml.AppendLine($"    <Author>{EscapeXml(item.Author)}</Author>");
                xml.AppendLine($"    <Year>{EscapeXml(item.Year)}</Year>");
                xml.AppendLine($"    <Location>{EscapeXml(item.Location)}</Location>");
                xml.AppendLine($"    <UsageConditions>{EscapeXml(item.UsageConditions)}</UsageConditions>");
                xml.AppendLine("  </Software>");
            }

            xml.AppendLine("</ArrayOfSoftware>");
            return xml.ToString();
        }

        private string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        // Кастомний StringWriter з підтримкою UTF-8
        private class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding _encoding;

            public StringWriterWithEncoding(Encoding encoding)
            {
                _encoding = encoding;
            }

            public override Encoding Encoding => _encoding;
        }
    }
}