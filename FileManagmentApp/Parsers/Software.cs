using System.Xml.Serialization;

namespace MauiApp24.Parsers
{
    public class Software
    {
        [XmlAttribute]
        public string Name { get; set; } = "";

        [XmlAttribute]
        public string Version { get; set; } = "";

        public string Annotation { get; set; } = "";
        public string Type { get; set; } = "";
        public string Author { get; set; } = "";
        public string Year { get; set; } = "";
        public string Location { get; set; } = "";
        public string UsageConditions { get; set; } = "";

        [XmlIgnore]
        public string FullName => $"{Name} {Version}".Trim();

        [XmlIgnore]
        public string Details => $"{Author} | {Year} | {Type}".Trim(' ', '|');
    }
}