using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransformHelpers.Transformers
{
    /// <summary>
    /// convert json string to xml
    /// todo: more...
    /// </summary>
    public static class JsonToXml
    {
        public static XmlDocument ToXml(string json) => JsonConvert.DeserializeXmlNode(json);
    }
}
