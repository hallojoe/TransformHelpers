using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace TransformHelpers.Transformers
{
    /// <summary>
    /// XML to JSON conversion
    /// </summary>
    public static class XmlToJson
    {

        const string XPATH_COMMENTS = "//comment()";
        const string REGEX_QUOTES_ON_NUMERIC_PROPERTY_VALUES = "\\\"([\\d\\.]+)\\\"";
        const string REGEX_QUOTES_ON_BOOLEAN_PROPERTY_VALUES = "\\\"(true|false)\\\"";
        const string REGEX_FIRST_CHAR_AD_SYMBOL = "(?<=\")(@)(?!.*\":\\s )";
        const string REGEX_FIRST_CHAR_HASH_SYMBOL = "(?<=\")(#)(?!.*\":\\s )";
        const string REGEX_GROUP_1 = "$1";
        const string EMPTY = "";

        /// <summary>
        /// Converts xml document to json string
        /// Inspirering sauce: https://github.com/SneakyBrian/XML2JSON/blob/master/XML2JSON.Core/Converter.cs
        /// </summary>
        /// <param name="xml">xml document</param>
        /// <returns>xml document converted to json</returns>
        public static string ToJson(XmlDocument doc, bool stripAttributeIndicators = true)
        {
            // strip comments
            var comments = doc.SelectNodes(XPATH_COMMENTS);
            if (comments != null)
                foreach (var node in comments.Cast<XmlNode>())
                    if (node.ParentNode != null)
                        node.ParentNode.RemoveChild(node);

            // serialize xml as json
            // note: int, bool etc. are quoted after serialilization
            var json = JsonConvert.SerializeXmlNode(doc.DocumentElement, Formatting.Indented);

            // no magic will happen here i terms of 
            // leaving out quotes on int, bool
            // lets be stupid and give that a cheap shot:
            json = UnQuoteUnquoteables(json);

            // strip the @ and # characters
            if (stripAttributeIndicators)
            {
                json = UnHashUnhashables(json);
                json = UnAdUnadables(json);
            }
            return json;
        }

        /// <summary>
        /// Converts xml string to json string
        /// </summary>
        /// <param name="xml">xml as string</param>
        /// <returns>xml string converted to json</returns>
        public static string ToJson(string xml, bool stripAttributeIndicators = true)
        {
            // load xml string to xml document
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return ToJson(doc, stripAttributeIndicators);
        }

        /// <summary>
        /// async xml document to json
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static async Task<string> ToJsonAsync(XmlDocument xml, bool stripAttributeIndicators = true) => 
            await Task<string>.Factory.StartNew(() => ToJson(xml, stripAttributeIndicators));

        /// <summary>
        /// async xml string to json
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static async Task<string> ConvertToJsonAsync(string xml, bool stripAttributeIndicators = true) => 
            await Task<string>.Factory.StartNew(() => ToJson(xml, stripAttributeIndicators));

        /// <summary>
        /// Remove double quotes on numeric values and booleans
        /// ex: "2017" => 2017
        /// ex: "20.17" => 20.17
        /// ex: "true" => true
        /// </summary>
        /// <param name="s"></param>
        /// <returns>Unquoted string</returns>
        internal static string UnQuoteUnquoteables(string s)
        {
            s = Regex.Replace(s, REGEX_QUOTES_ON_NUMERIC_PROPERTY_VALUES, REGEX_GROUP_1, RegexOptions.CultureInvariant);
            s = Regex.Replace(s, REGEX_QUOTES_ON_BOOLEAN_PROPERTY_VALUES , REGEX_GROUP_1, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            return s;
        }

        /// <summary>
        /// Removes # from json property names
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string UnHashUnhashables(string s) => 
            Regex.Replace(s, REGEX_FIRST_CHAR_HASH_SYMBOL, EMPTY, RegexOptions.IgnoreCase);

        /// <summary>
        /// Removes @ from json property names
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static string UnAdUnadables(string s) =>
            Regex.Replace(s, REGEX_FIRST_CHAR_AD_SYMBOL, EMPTY, RegexOptions.IgnoreCase);

    }
}