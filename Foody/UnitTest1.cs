using Foody.Models;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;



namespace Foody
{
    [TestFixture]
    public class FoodyTests
    {
        private RestClient client;
        private static string createdFoodId;
        private const string baseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";

        [OneTimeSetUp]
        public void Setup()
        {

            string token = GetJwtToken("yoana5yoana5", "yoana5yoana5");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)

            };

            client = new RestClient(options);
        }


        private string GetJwtToken(string username, string password)
        {

            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication",Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

           
        }


        [Test, Order (1)]

        public void CreateFood_ShouldReturnCreated()
        {
            var food = new 
            {
                Name = "New Food",
                Description = "Delicious new food item.",
                Url = ""
            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);

            var response = client.Execute(request);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdFoodId = json.GetProperty("foodId").GetString() ?? string.Empty;

            Assert.That(createdFoodId, Is.Not.Null.And.Not.Empty, "Food ID should not be null or empty.");

        }


        [Test, Order(2)]


        public void EditFoodTitle_ShouldReturnOk()
        {
            var changes = new[]
            {
               new {path = "/name", op = "replace", value = "Updated food name"}
            };

            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch);

            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));

            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }



        [Test, Order(3)]



        public void GetAllFoods_ShouldReturnList()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);


            var response = client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.OK));

            var foods = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(foods, Is.Not.Empty);
           

        }



        [Test, Order(4)]


        public void DeleteFood_ShouldReturnOK()
        {
            var request = new RestRequest($"/api/Food/Delete/{createdFoodId}", Method.Delete);

            var response = client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));

        }



        [Test, Order(5)]


        public void CreateFood_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var food = new 
            {
                Name = "",
                Description = ""

            };

            var request = new RestRequest("/api/Food/Create", Method.Post);
            request.AddJsonBody(food);
            var response = client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo((HttpStatusCode)HttpStatusCode.BadRequest));

        }


        [Test, Order(6)]



        public void EditNonExistingFood_ShouldReturnNotFound()
        {
            string fakeID = "123";
            var changes = new[]
            {
               new {path = "/name", op = "replace", value = "New Title"}
            };

            var request = new RestRequest($"/api/Food/Edit/{fakeID}", Method.Patch);
         
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(response.Content, Does.Contain("No food revues..."));


        }




        [Order(7)]
        [Test]

        public void DeleteNonExistingFood_ShouldReturnBadRequest()
        {
            string fakeID = "123";

            var request = new RestRequest($"/api/Food/Delete/{fakeID}", Method.Delete);
            var response = client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this food revue!"));

        }


        


        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}