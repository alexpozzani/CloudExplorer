using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.IO;

namespace CloudExplorer.IntegrationTests
{
    public class DataServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public DataServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetContent_ReturnsOk()
        {
            var payload = JsonConvert.SerializeObject(new { path = "" });
            var response = await _client.PostAsync("/api/dataservice/content", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task CreateDir_ReturnsOk()
        {
            var payload = JsonConvert.SerializeObject(new { parent = new { path = "" }, name = "TestDir" });
            var response = await _client.PostAsync("/api/dataservice/createdir", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Rename_ReturnsOk()
        {
            var payload = JsonConvert.SerializeObject(new { target = new { path = "TestDir" }, newName = "RenamedDir" });
            var response = await _client.PostAsync("/api/dataservice/rename", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Delete_ReturnsOk()
        {
            var payload = JsonConvert.SerializeObject(new[] { new { path = "RenamedDir" } });
            var response = await _client.PostAsync("/api/dataservice/delete", new StringContent(payload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Upload_ReturnsOk()
        {
            // Create test file content
            var fileContent = "This is a test file content for upload integration test.";
            var fileName = "test-upload-file.txt";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);

            // Create multipart form data content
            using var form = new MultipartFormDataContent();
            
            // Add the parent path as form data
            var parentJson = JsonConvert.SerializeObject(new { path = "" });
            form.Add(new StringContent(parentJson, Encoding.UTF8, "application/json"), "Parent");
            
            // Add the file
            using var fileStream = new MemoryStream(fileBytes);
            using var fileContent2 = new StreamContent(fileStream);
            fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            form.Add(fileContent2, "Files", fileName);

            // Send the upload request
            var response = await _client.PostAsync("/api/dataservice/upload", form);
            response.EnsureSuccessStatusCode();

            // Verify the response content
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.NotNull(result);
            Assert.NotNull(result?.uploaded);
            var uploadedArray = result?.uploaded as Newtonsoft.Json.Linq.JArray;
            Assert.NotNull(uploadedArray);
            Assert.True(uploadedArray.Count > 0);
        }

        [Fact]
        public async Task UploadToSubdirectory_ReturnsOk()
        {
            // First, create a subdirectory for the upload test
            var createDirPayload = JsonConvert.SerializeObject(new { parent = new { path = "" }, name = "UploadTestDir" });
            var createDirResponse = await _client.PostAsync("/api/dataservice/createdir", new StringContent(createDirPayload, Encoding.UTF8, "application/json"));
            createDirResponse.EnsureSuccessStatusCode();

            // Create test file content for subdirectory upload
            var fileContent = "This is a test file uploaded to a subdirectory.";
            var fileName = "subdirectory-upload-test.txt";
            var fileBytes = Encoding.UTF8.GetBytes(fileContent);

            // Create multipart form data content
            using var form = new MultipartFormDataContent();
            
            // Add the parent path as form data (pointing to the subdirectory)
            var parentJson = JsonConvert.SerializeObject(new { path = "UploadTestDir" });
            form.Add(new StringContent(parentJson, Encoding.UTF8, "application/json"), "Parent");
            
            // Add the file
            using var fileStream = new MemoryStream(fileBytes);
            using var fileContent2 = new StreamContent(fileStream);
            fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
            form.Add(fileContent2, "Files", fileName);

            // Send the upload request to subdirectory
            var uploadResponse = await _client.PostAsync("/api/dataservice/upload", form);
            uploadResponse.EnsureSuccessStatusCode();

            // Verify the upload response content
            var responseContent = await uploadResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
            
            Assert.NotNull(result);
            Assert.NotNull(result?.uploaded);
            var uploadedArray = result?.uploaded as Newtonsoft.Json.Linq.JArray;
            Assert.NotNull(uploadedArray);
            Assert.True(uploadedArray.Count > 0);
            
            // Verify the uploaded file path contains the subdirectory
            var uploadedPath = uploadedArray[0]?.ToString();
            Assert.NotNull(uploadedPath);
            Assert.Contains("UploadTestDir", uploadedPath);

            // Clean up: delete the test directory and its contents
            var deletePayload = JsonConvert.SerializeObject(new[] { new { path = "UploadTestDir" } });
            var deleteResponse = await _client.PostAsync("/api/dataservice/delete", new StringContent(deletePayload, Encoding.UTF8, "application/json"));
            deleteResponse.EnsureSuccessStatusCode();
        }
    }
}
