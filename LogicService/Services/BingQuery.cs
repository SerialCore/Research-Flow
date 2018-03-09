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

        public static async Task<List<BingResult>> QueryAsync(string text, int count, Action<List<BingResult>> onQueryCompleted = null, 
            BingCountry country = BingCountry.UnitedStates, BingLanguage language = BingLanguage.English, BingQueryType type = BingQueryType.Search)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var searchConfig = new BingSearchConfig
            {
                Country = country,
                Language = language,
                Query = text,
                QueryType = type
            };

            List<BingResult> result = await BingService.Instance.RequestAsync(searchConfig, count);

            if (onQueryCompleted != null)
            {
                onQueryCompleted(result);
            }

            return result;
        }

    }
}
