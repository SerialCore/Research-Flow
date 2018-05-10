using Microsoft.Toolkit.Services.Bing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Services
{
    public class BingQuery
    {

        public static async void QueryAsync(string text, int count, Action<List<BingResult>> onQueryCompleted = null, Action<string> onError = null, 
            BingCountry country = BingCountry.UnitedStates, BingLanguage language = BingLanguage.English, BingQueryType type = BingQueryType.Search)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var searchConfig = new BingSearchConfig
            {
                Country = country,
                Language = language,
                Query = text,
                QueryType = type
            };
            
            try
            {
                List<BingResult> result = await BingService.Instance.RequestAsync(searchConfig, count);
                if (onQueryCompleted != null)
                {
                    onQueryCompleted(result);
                }
            }
            catch (WebException webEx)
            {
                if (onError != null)
                {
                    onError(webEx.Message);
                }
            }
            catch (Exception e)
            {
                if (onError != null)
                {
                    onError(e.Message);
                }
            }
        }

    }
}
