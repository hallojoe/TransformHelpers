using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TransformHelpers.Transformers
{
    public static class Extensions
    {

        const char TREAT_AS_XML_IF_STARTS_WITH = '<';

        public enum StringType
        {
            Undefined = 0, 
            Csv = 1, 
            Xml = 2
        }

        public static string ToJson(this string s, StringType type)
        {
            if (type == StringType.Csv)
                return CsvToJson.ToJson(s);
            if (type == StringType.Xml)
                return XmlToJson.ToJson(s);
            return string.Empty;
        }

        /// <summary>
        /// An enthusiastic guy trying to guess string input(csv|xml)
        /// </summary>
        /// <param name="csvOrXmlString"></param>
        /// <param name="throwException"></param>
        /// <returns></returns>
        public static string ToJson(this string csvOrXmlString, bool throwException = false)
        {
            try
            {
                if (csvOrXmlString[0] != TREAT_AS_XML_IF_STARTS_WITH)
                    return ToJson(csvOrXmlString, StringType.Csv);
            else
                return ToJson(csvOrXmlString, StringType.Xml);
            }
            catch (Exception ex)
            {
                if(!throwException)
                    return "{ error: '" + ex.Message + "'";
                throw;
            }
        }

        public static string ToJson(this XmlDocument doc) => XmlToJson.ToJson(doc);
        public static XmlDocument ToXml(this string json) => JsonToXml.ToXml(json);

    }
}
