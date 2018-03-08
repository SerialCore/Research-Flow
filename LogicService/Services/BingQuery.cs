using Microsoft.Toolkit.Services.Bing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Services
{
    public class BingQuery
    {

        /// <summary>
        /// UnitedStates-English-Search
        /// </summary>
        /// <param name="text">Query Text</param>
        /// <param name="count">Result Count</param>
        /// <returns></returns>
        public static async Task<List<BingResult>> QueryAsync(string text, int count)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var searchConfig = new BingSearchConfig
            {
                Country = BingCountry.UnitedStates,
                Language = BingLanguage.English,
                Query = text,
                QueryType = BingQueryType.Search
            };

            return await BingService.Instance.RequestAsync(searchConfig, count);
        }

        /// <summary>
        /// Country-Language-Search
        /// </summary>
        /// <param name="text">Query Text</param>
        /// <param name="count">Result Count</param>
        /// <returns></returns>
        public static async Task<List<BingResult>> QueryAsync(string text, int count, BingCountry country, BingLanguage language)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var searchConfig = new BingSearchConfig
            {
                Country = BingCountry.UnitedStates,
                Language = BingLanguage.English,
                Query = text,
                QueryType = BingQueryType.Search
            };

            return await BingService.Instance.RequestAsync(searchConfig, count);
        }

        /// <summary>
        /// Country-Language-QueryType
        /// </summary>
        /// <param name="text">Query Text</param>
        /// <param name="count">Result Count</param>
        /// <returns></returns>
        public static async Task<List<BingResult>> QueryAsync(string text, int count, BingCountry country, BingLanguage language, BingQueryType type)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var searchConfig = new BingSearchConfig
            {
                Country = BingCountry.UnitedStates,
                Language = BingLanguage.English,
                Query = text,
                QueryType = BingQueryType.Search
            };

            return await BingService.Instance.RequestAsync(searchConfig, count);
        }

    }
}
