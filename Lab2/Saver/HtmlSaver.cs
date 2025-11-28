using MauiApp24.Parsers;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Text;

namespace Saver
{
    public class HtmlSaver : ISaver
    {
        private string _customXslt;

        public HtmlSaver(string customXslt = null)
        {
            _customXslt = customXslt;
        }

        public string GenerateContent(List<Software> data)
        {
            try
            {
                // Генеруємо XML з атрибутами
                string xmlData = GenerateXml(data);
                string xsltTemplate = _customXslt ?? GetDefaultXslt();

                using var xmlReader = XmlReader.Create(new StringReader(xmlData));
                using var xsltReader = XmlReader.Create(new StringReader(xsltTemplate));
                using var stringWriter = new StringWriter();

                var transform = new XslCompiledTransform();
                transform.Load(xsltReader);
                transform.Transform(xmlReader, null, stringWriter);

                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                return GenerateSimpleHtml(data);
            }
        }

        private string GenerateXml(List<Software> data)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var writer = XmlWriter.Create(stringWriter, settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("ArrayOfSoftware");

            foreach (var software in data)
            {
                writer.WriteStartElement("Software");
                writer.WriteAttributeString("Name", EscapeXml(software.Name));
                writer.WriteAttributeString("Version", EscapeXml(software.Version));
                writer.WriteAttributeString("Annotation", EscapeXml(software.Annotation));
                writer.WriteAttributeString("Type", EscapeXml(software.Type));
                writer.WriteAttributeString("Author", EscapeXml(software.Author));
                writer.WriteAttributeString("Year", EscapeXml(software.Year));
                writer.WriteAttributeString("Location", EscapeXml(software.Location));
                writer.WriteAttributeString("UsageConditions", EscapeXml(software.UsageConditions));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();

            return stringWriter.ToString();
        }

        private string GetDefaultXslt()
        {
            return @"<?xml version='1.0' encoding='UTF-8'?>
<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>
  <xsl:template match='/ArrayOfSoftware'>
    <html>
    <head>
        <title>Software Report</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            table { border-collapse: collapse; width: 100%; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
            th { background-color: #42A5F5; color: white; }
            tr:nth-child(even) { background-color: #f2f2f2; }
            h2 { color: #42A5F5; }
        </style>
    </head>
    <body>
      <h2>Звіт про програмне забезпечення</h2>
      <table>
        <tr>
            <th>Назва</th>
            <th>Версія</th>
            <th>Автор</th>
            <th>Тип</th>
            <th>Рік</th>
        </tr>
        <xsl:for-each select='Software'>
          <tr>
            <td><xsl:value-of select='@Name'/></td>
            <td><xsl:value-of select='@Version'/></td>
            <td><xsl:value-of select='@Author'/></td>
            <td><xsl:value-of select='@Type'/></td>
            <td><xsl:value-of select='@Year'/></td>
          </tr>
        </xsl:for-each>
      </table>
      <div style='margin-top: 20px; text-align: center; color: #666;'>
        Кількість записів: <xsl:value-of select='count(Software)'/>
      </div>
    </body>
    </html>
  </xsl:template>
</xsl:stylesheet>";
        }

        private string GenerateSimpleHtml(List<Software> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <title>Software Report</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 20px; }");
            sb.AppendLine("        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }");
            sb.AppendLine("        th { background-color: #42A5F5; color: white; }");
            sb.AppendLine("        tr:nth-child(even) { background-color: #f2f2f2; }");
            sb.AppendLine("        h2 { color: #42A5F5; }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("    <h2>Звіт про програмне забезпечення</h2>");
            sb.AppendLine("    <table>");
            sb.AppendLine("        <tr><th>Назва</th><th>Версія</th><th>Автор</th><th>Тип</th><th>Рік</th></tr>");

            foreach (var software in data)
            {
                sb.AppendLine("        <tr>");
                sb.AppendLine($"            <td>{EscapeHtml(software.Name)}</td>");
                sb.AppendLine($"            <td>{EscapeHtml(software.Version)}</td>");
                sb.AppendLine($"            <td>{EscapeHtml(software.Author)}</td>");
                sb.AppendLine($"            <td>{EscapeHtml(software.Type)}</td>");
                sb.AppendLine($"            <td>{EscapeHtml(software.Year)}</td>");
                sb.AppendLine("        </tr>");
            }

            sb.AppendLine("    </table>");
            sb.AppendLine($"    <div style='margin-top: 20px; text-align: center; color: #666;'>");
            sb.AppendLine($"        Кількість записів: {data.Count}");
            sb.AppendLine("    </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
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

        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}