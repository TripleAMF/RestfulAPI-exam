using Newtonsoft.Json;

namespace MovieCatalogTests
{
    public class MovieDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class ApiResponseDto
    {
        [JsonProperty("msg")]
        public string Msg { get; set; }

        [JsonProperty("movie")]
        public MovieDto Movie { get; set; }
    }
}
