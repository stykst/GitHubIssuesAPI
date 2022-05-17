using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHubIssuesAPI.Test
{
    public class Tests
    {
        private RestClient client;
        private RestRequest request;
        const string user = "testnakov";
        const string repo = "test-nakov-repo";
        const string apiToken = "api-token";
        const string baseUrl = "https://api.github.com";
        string url = $"/repos/{user}/{repo}/issues";

        [SetUp]
        public void Setup()
        {
            client = new RestClient(baseUrl);
            client.Authenticator = new HttpBasicAuthenticator(user, apiToken);
            request = new RestRequest(url);
        }

        [Test]
        public async Task Test_CreateIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);


            // Assert
            Assert.AreEqual(HttpStatusCode.Created, issue.statusCode);
            Assert.Greater(issue.id, 0);
            Assert.Greater(issue.number, 0);
            Assert.IsNotEmpty(issue.title);
        }

        [Test]
        public async Task Test_TryToCreateIssue_NoAuthorization()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New body Kris_Test";
            client.Authenticator = new HttpBasicAuthenticator(user, "");

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, issue.statusCode);
        }

        [Test]
        public async Task Test_TryToCreateIssue_NoTitle()
        {
            // Arrange
            var issueTitle = string.Empty;
            var issueBody = "New comment body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);

            // Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, issue.statusCode);
        }

        [Test]
        public async Task Test_CreateCommentForIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var body = "New body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var comment = await CreateComment(body, issue.number);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, comment.statusCode);
            Assert.That(comment, Is.Not.Null);
            Assert.AreEqual(body, comment.body);
        }

        [Test]
        public async Task Test_GetAllIssues()
        {
            // Act
            var response = await client.ExecuteAsync(request);
            var issues = JsonSerializer.Deserialize<List<Issue>>(response.Content);

            // Assert
            Assert.IsNotNull(response.Content);
            Assert.That(issues.Count > 1);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            foreach (var issue in issues)
            {
                Assert.Greater(issue.id, 0);
                Assert.Greater(issue.number, 0);
                Assert.IsNotEmpty(issue.title);
                Assert.IsNotEmpty(issue.body);
            }
        }

        [Test]
        public async Task Test_GetIssueByNumber()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/{issue.number}");
            var response = await client.ExecuteAsync(request);
            var newIssue = JsonSerializer.Deserialize<Issue>(response.Content);

            // Assert
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(issue.number, newIssue.number);
        }

        [Test]
        public async Task Test_TryTo_GetIssueByInvalidNumber()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/0");
            var response = await client.ExecuteAsync(request);

            // Assert
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Test]
        public async Task Test_GetAllLabelsForIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/{issue.number}/labels");
            var response = await client.ExecuteAsync(request);
            var result = JsonSerializer.Deserialize<List<Label>>(response.Content);

            // Assert
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(typeof(List<Label>), result.GetType());
        }

        [Test]
        public async Task Test_GetAllCommentsForIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/{issue.number}/comments");
            var response = await client.ExecuteAsync(request);
            var comment = JsonSerializer.Deserialize<List<Comment>>(response.Content);

            // Assert
            Assert.IsNotNull(response.Content);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(typeof(List<Comment>), comment.GetType());
        }

        [Test]
        public async Task Test_GetCommentForIssueById()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var body = issueBody;

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var comment = await CreateComment(body, issue.number);
            var newRequest = new RestRequest($"{url}/comments/{comment.id}");
            var newResponse = await client.ExecuteAsync(newRequest);
            var newComment = JsonSerializer.Deserialize<Comment>(newResponse.Content);

            // Assert
            Assert.IsNotNull(newResponse.Content);
            Assert.AreEqual(HttpStatusCode.OK, newResponse.StatusCode);
            Assert.AreEqual(comment.id, newComment.id);
        }

        [Test]
        public async Task Test_UpdateIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var title = "QWERTY";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/{issue.number}");
            request.AddBody(new { title });
            var response = await client.ExecuteAsync(request, Method.Patch);
            var newIssue = JsonSerializer.Deserialize<Issue>(response.Content);


            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotEmpty(issue.title);
            Assert.AreEqual(title, newIssue.title);
        }

        [Test]
        public async Task Test_CloseExisitngIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var state = "closed";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var request = new RestRequest($"{url}/{issue.number}");
            request.AddBody(new { state });
            var response = await client.ExecuteAsync(request, Method.Patch);
            var newIssue = JsonSerializer.Deserialize<Issue>(response.Content);


            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotEmpty(issue.state);
            Assert.AreEqual(state, newIssue.state);
        }

        [Test]
        public async Task Test_UpdateCommentForIssueById()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var commentBody = "New comment body Kris_Test";
            var body = "The Comment is New";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var comment = await CreateComment(commentBody, issue.number);
            var newRequest = new RestRequest($"{url}/comments/{comment.id}");
            newRequest.AddBody(new { body });
            var newResponse = await client.ExecuteAsync(newRequest, Method.Patch);
            var newComment = JsonSerializer.Deserialize<Comment>(newResponse.Content);

            // Assert
            Assert.IsNotNull(newComment);
            Assert.AreEqual(HttpStatusCode.OK, newResponse.StatusCode);
            Assert.AreEqual(body, newComment.body);
        }

        [Test]
        public async Task Test_DeleteCommentForIssue()
        {
            // Arrange
            var issueTitle = "New issue Kris_Test";
            var issueBody = "New comment body Kris_Test";
            var body = "New body Kris_Test";

            // Act
            var issue = await CreateIssue(issueTitle, issueBody);
            var comment = await CreateComment(body, issue.number);
            await DeleteComment(comment);
            var newRequest = new RestRequest($"{url}/comments/{comment.id}");
            var newResponse = await client.ExecuteAsync(newRequest);
            var newComment = JsonSerializer.Deserialize<Comment>(newResponse.Content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, comment.statusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, newResponse.StatusCode);
        }

        private async Task DeleteComment(Comment comment)
        {
            var newRequest = new RestRequest($"{url}/comments/{comment.id}");
            var newResponse = await client.ExecuteAsync(newRequest, Method.Delete);
        }

        private async Task<Issue> CreateIssue(string title, string body)
        {
            var request = new RestRequest(url);
            request.AddBody(new { body, title });
            var response = await client.ExecuteAsync(request, Method.Post);
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);
            issue.statusCode = response.StatusCode;
            return issue;
        }

        private async Task<Comment> CreateComment(string body, int issueNumber)
        {
            var request = new RestRequest($"{url}/{issueNumber}/comments");
            request.AddBody(new { body });
            var response = await client.ExecuteAsync(request, Method.Post);
            var comment = JsonSerializer.Deserialize<Comment>(response.Content);
            comment.statusCode = response.StatusCode;
            return comment;
        }
    }
}