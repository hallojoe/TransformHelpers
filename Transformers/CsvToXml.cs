using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransformHelpers.Transformers
{
    public static class CsvToXml
    {
        /// <summary>
        /// convert CSV to xml
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static string ToXml(string csv)
        {
            using (var textReader = new StringReader(csv))
            using (var csvReader = new CsvReader(textReader))
            {
                var o = csvReader.GetRecords<dynamic>();
                var xmlSerializer = new System.Xml.Serialization.XmlSerializer(o.GetType());
                using (var sw = new StringWriter()) {
                    using (var tw = new XmlTextWriter(sw))
                    {
                        xmlSerializer.Serialize(tw, o);
                        return sw.ToString();
                    }
                }
            }
        }

        /// <summary>
        /// convert CSV to xml async
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static async Task<string> ToXmlAsync(string csv) =>
            await Task<string>.Factory.StartNew(() => ToXml(csv));

    }
}
