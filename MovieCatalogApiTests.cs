using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;

namespace MovieCatalogTests
{
    [TestFixture]
    public class MovieCatalogApiTests
    {
        private RestClient _client;
        private static string _createdMovieId;

        private const string BaseUrl = "http://144.91.123.158:5000/api";
        private const string Email = "aleks147852@abv.bg";
        private const string Password = "aleks147852258741";

        [OneTimeSetUp]
        public void Setup()
        {
            var tempClient = new RestClient(BaseUrl);

            var loginRequest = new RestRequest("/User/Authentication", Method.Post);
            loginRequest.AddJsonBody(new { email = Email, password = Password });

            var loginResponse = tempClient.Execute(loginRequest);
            var loginData = JsonConvert.DeserializeObject<dynamic>(loginResponse.Content);
            string token = loginData["accessToken"].ToString();

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            _client = new RestClient(options);
        }

        [Test, Order(1)]
        public void CreateMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "Test Movie",
                description = "Test Description"
            });

            var response = _client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(responseData.Movie, Is.Not.Null);
            Assert.That(responseData.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(responseData.Msg, Is.EqualTo("Movie created successfully!"));

            _createdMovieId = responseData.Movie.Id;
        }

        [Test, Order(2)]
        public void EditMovie_WithValidId_ShouldReturnSuccess()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", _createdMovieId);
            request.AddJsonBody(new
            {
                title = "Edited Movie Title",
                description = "Edited Movie Description"
            });

            var response = _client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public void GetAllMovies_ShouldReturnNonEmptyList()
        {
            var request = new RestRequest("/Catalog/All", Method.Get);

            var response = _client.Execute(request);
            var movies = JsonConvert.DeserializeObject<List<MovieDto>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(movies, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteMovie_WithValidId_ShouldReturnSuccess()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", _createdMovieId);

            var response = _client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateMovie_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                posterUrl = "",
                trailerLink = ""
            });

            var response = _client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditMovie_WithNonExistingId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "nonexistentid123");
            request.AddJsonBody(new
            {
                title = "Ghost Movie",
                description = "Ghost Description"
            });

            var response = _client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(responseData.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public void DeleteMovie_WithNonExistingId_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "nonexistentid123");

            var response = _client.Execute(request);
            var responseData = JsonConvert.DeserializeObject<ApiResponseDto>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.BadRequest));
            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client?.Dispose();
        }
    }
}
