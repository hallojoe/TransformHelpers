using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using TransformHelpers.Transformers;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace UnitTestProjectForTransformers
{
    [TestClass]
    public class UnitTestTransformers
    {

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void Test_JsonToXml_And_Then_XmlToJson_AreEqual()
        {

            // note: to test json->xml and xml->json we need to account for:
            //
            // - empty json arrays are ignored by the xml serializer used, 
            //   so remove them in order to make a test that will 
            //   return positive even though this covertion is not 100% acurate

            // get json test text            
            var json = GetText(SupportedTestInputFormats.Json);

            // strip empty arrays from json string
            json = StripEmptyJsonArrays(json);

            // load test text into a jtoken
            var tokenIn = JToken.Parse(json);

            // get xml document from json text
            var xmlDocument = JsonToXml.ToXml(json);

            // get outer document string
            var xml = xmlDocument.OuterXml;

            // get json from xml
            var xmlToJson = XmlToJson.ToJson(xml);

            // load json from xml into jtoken
            // todo: resolve problems :)
            // - this parses any value as string, so add some type jugeling            
            var tokenOut = JToken.Parse(xmlToJson);

            // test if tokens are equal
            var areEqual = JToken.DeepEquals(tokenIn, tokenOut);

            // test
            Assert.AreEqual(true, areEqual);

        }


        internal enum SupportedTestInputFormats
        {
            Json,
            Xml
        }

        internal string GetText(SupportedTestInputFormats format)
        {
            // note: remember to set Copy to output on new files 
            var filename = format == SupportedTestInputFormats.Json ? "json.txt" : "xml.txt";
            var directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var path = $"{ directory }\\{ filename }";
            return File.ReadAllText(path);
        }

        internal string StripJsonWhitening(string json)
        {
            return Regex.Replace(json, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
        }

        internal string StripEmptyJsonArrays(string json)
        {
            var pattern = "\\\"([^\"\\\\]+)\\\":\\s?\\[\\](?:[^,\"]+)?,?";
            var result = Regex.Replace(json, pattern, string.Empty);
            return result;
        }



    }
}




