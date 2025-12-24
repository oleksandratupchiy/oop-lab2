using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saver
{
    public static class SaverFactory
    {
        public static ISaver GetSaver(string format)
        {
            return format switch
            {
                "XML" => new XmlSaver(),
                "HTML" => new HtmlSaver(),
                _ => throw new ArgumentException("Invalid format")
            };
        }
    }

}
