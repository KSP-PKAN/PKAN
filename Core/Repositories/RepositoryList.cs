using Newtonsoft.Json;

using CKAN.Games;

namespace CKAN
{
    public struct RepositoryList
    {
        public Repository[] repositories;

        public static RepositoryList? DefaultRepositories(IGame game, string? userAgent)
        {
            try
            {
                return JsonConvert.DeserializeObject<RepositoryList>(
                    Net.DownloadText(game.RepositoryListURL, userAgent) ?? "");
            }
            catch
            {
                return default;
            }
        }
    }
}
