using System.Net;
using System.Text.RegularExpressions;

namespace YouTube_Thumb_Scraper
{
    internal class Program
    {
        private const string SEARCH_URL = "https://www.youtube.com/results?search_query=gaming";
        private const string URL = "https://www.youtube.com";

        private static async Task Main()
        {
            foreach (var url in await GetVideoLinks())
            {
                Console.WriteLine(url.Key);
                Console.WriteLine(url.Value);
            }
        }

        private static async Task<Dictionary<string, string>> GetVideoLinks()
        {
            Dictionary<string, string> temp = new();

            var client = new HttpClient();
            var response = await client.GetAsync(URL);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
                var html = await reader.ReadToEndAsync();

                Regex regex = new("watch\\?v=(\\S{11})");
                MatchCollection matches = regex.Matches(html);

                foreach (var match in matches.Cast<Match>())
                {
                    var param = match.Groups[1].Value;
                    var fullUrl = "https://www.youtube.com/watch?v=" + param;

                    if (!temp.ContainsKey(fullUrl))
                        temp.Add(fullUrl, await GetThumbnails(param));
                }
            }

            return temp;
        }

        private static async Task<string> GetThumbnails(string videoId)
        {
            var thumbnailUrl = "https://img.youtube.com/vi/" + videoId + "/maxresdefault.jpg";

            using var client = new HttpClient();

            var response = await client.GetAsync(thumbnailUrl);

            var responseStream = await response.Content.ReadAsStreamAsync();

            var filename = videoId + ".jpg";

            await using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                await responseStream.CopyToAsync(fileStream);
            }

            Console.WriteLine("Thumbnail successfully downloaded!");

            return videoId;
        }
    }
}
