using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;
using CsvHelper;
using System.Xml;
using System.Xml;

namespace TransformHelpers.Transformers
{
    /// <summary>
    /// handles CSV to JSON convertion
    /// </summary>
    public static class CsvToJson
    {
        /// <summary>
        /// convert CSV string to JSON string
        /// </summary>
        /// <param name="csv"></param>
        /// <returns>JSON data as string</returns>
        public static string ToJson(string csv)
        {
            using (var textReader = new StringReader(csv))
                using (var csvReader = new CsvReader(textReader))
                    return JsonConvert.SerializeObject(csvReader.GetRecords<dynamic>(), Formatting.Indented);
        }

        /// <summary>
        /// async version of conversion function
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static async Task<string> ToJsonAsync(string csv) => await Task<string>.Factory.StartNew(() => ToJson(csv));

    }
}