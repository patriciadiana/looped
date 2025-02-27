﻿using backend.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace backend.Controllers
{
    [ApiController]
    [Route("main")]
    public class MainController(HttpClient _httpClient, AuthService _authService, DatabaseService _databaseService) : ControllerBase
    {
        private string RavelryApiUrl = "https://api.ravelry.com";

        [HttpGet("current_user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/current_user.json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var userData = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(userData);
            var currentUserUsername = jsonDocument.RootElement.GetProperty("user").GetProperty("username").GetString();
            Console.Write(currentUserUsername);

            url = $"{RavelryApiUrl}/people/{currentUserUsername}.json";
            request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            userData = await response.Content.ReadAsStringAsync();

            return Ok(JsonSerializer.Deserialize<object>(userData));
        }

        [HttpGet("user/{user_name}")]
        public async Task<IActionResult> GetCurrentUser(string user_name)
        {
            string accessToken = _authService.getToken();

            var url = $"{RavelryApiUrl}/people/{user_name}.json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var userData = await response.Content.ReadAsStringAsync();

            return Ok(JsonSerializer.Deserialize<object>(userData));
        }

        [HttpPost("user/{user_name}")]
        public async Task<IActionResult> UpdateCurrentUser(string user_name)
        {
            string accessToken = _authService.getToken();

            var url = $"{RavelryApiUrl}/people/{user_name}.json";

            var user = await _databaseService.GetUserByUsername(user_name);

            var payload = new
            {
                Data = user
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            // Return the response from the third-party API
            var responseData = await response.Content.ReadAsStringAsync();
            return Ok(JsonSerializer.Deserialize<object>(responseData));
        }
        [HttpGet("user/{user_name}/friendslist")]
        public async Task<IActionResult> CurrentUserFriends(string user_name)
        {
            string accessToken = _authService.getToken();

            var url = $"{RavelryApiUrl}/people/{user_name}/friends/list.json";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve projects.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving projects.", details = ex.Message });
            }
        }

        [HttpGet("current_user_api/{userCode}")]
        public async Task<IActionResult> GetCurrentUserApi(String userCode)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/current_user.json";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            var userData = await response.Content.ReadAsStringAsync();

            var jsonDocument = JsonDocument.Parse(userData);
            var currentUserUsername = jsonDocument.RootElement.GetProperty("user").GetProperty("username").GetString();
            Console.Write(currentUserUsername);

            url = $"{RavelryApiUrl}/people/{currentUserUsername}.json";
            request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            userData = await response.Content.ReadAsStringAsync();
            jsonDocument = JsonDocument.Parse(userData);

            var userId = jsonDocument.RootElement.GetProperty("user").GetProperty("id").GetInt64();
            var firstName = jsonDocument.RootElement.GetProperty("user").GetProperty("first_name").GetString();
            var location = jsonDocument.RootElement.GetProperty("user").GetProperty("location").GetString();
            var profilePic = jsonDocument.RootElement.GetProperty("user").GetProperty("large_photo_url").GetString();
            var aboutMe = jsonDocument.RootElement.GetProperty("user").GetProperty("about_me").GetString();
            var fave_colors = "";

            Console.WriteLine("USER ID FRATILOR " + userId);

            await _databaseService.UpdateUser(userCode, firstName,location,profilePic, currentUserUsername, aboutMe, fave_colors);

            return Ok(JsonSerializer.Deserialize<object>(userData));
        }

        [HttpGet("current_user_db/{userCode}")]
        public async Task<IActionResult> GetCurrentUserDatabase(String userCode)
        {
            Console.WriteLine($"user code {userCode}");
            var user = await _databaseService.GetUserById(userCode);

            var response = new
            {
                user = new
                {
                    id = user.Id,
                    first_name = user.first_name,
                    location = user.location,
                    large_photo_url = user.large_photo_url,
                    about_me = user.about_me,
                    username = user.username
                }
            };

            return Ok(response);

        }
        [HttpPost("update_profile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] string user_name, [FromQuery] string? firstName, 
            [FromQuery] string? location, [FromQuery]  string? aboutMe, [FromQuery] string? faveColors)
        {
            var user = await _databaseService.GetUserByUsername(user_name);
            await _databaseService.UpdateUser(user.code, firstName, location, user.large_photo_url, user_name, aboutMe, faveColors);
            user = await _databaseService.GetUserByUsername(user_name);


            string accessToken = _authService.getToken();

            var url = $"{RavelryApiUrl}/people/{user_name}.json";

            var payload = new
            {
                about_me = user.about_me,
                fave_colors = user.fave_colors,
                first_name = user.first_name
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            // Return the response from the third-party API
            var responseData = await response.Content.ReadAsStringAsync();



            Console.WriteLine($"!! {responseData}");

            return Ok(JsonSerializer.Deserialize<object>(responseData));
        }

        [HttpPost("add_project")]
        public async Task<IActionResult> AddProject([FromQuery] string user_name, [FromQuery] string? craft,
            [FromQuery] string? projectName, [FromQuery] string? usedAPattern, [FromQuery] string? patternUsed, [FromQuery] string? madeFor)
        {
            Console.WriteLine($"{user_name} {craft} {projectName} {madeFor}");

            string accessToken = _authService.getToken();

            var url = $"{RavelryApiUrl}/projects/{user_name}/create.json";

            var payload = new
            {
                craft_id = craft,
                made_for = madeFor,
                name = projectName,
                started = DateTime.Now
            };

            var jsonPayload = JsonSerializer.Serialize(payload);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());

            // Return the response from the third-party API
            var responseData = await response.Content.ReadAsStringAsync();



            Console.WriteLine($"!! {responseData}");

            return Ok(JsonSerializer.Deserialize<object>(responseData));
        }

        [HttpGet("user/{user_name}/projects")]
        public async Task<IActionResult> GetProjectsForCurrentUser(string user_name)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/projects/{user_name}/list.json";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve projects.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving projects.", details = ex.Message });
            }
        }
        [HttpGet("queue/delete")]
        public async Task<IActionResult> DeleteProjectFromQueue([FromQuery] string user_name, [FromQuery] int project_id)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);
            Console.WriteLine(" user name " + user_name + " project id " + project_id);

            var url = $"{RavelryApiUrl}/people/{user_name}/queue/{project_id}.json";

            Console.WriteLine(url);

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("da " + projectsData);
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("nu " + errorResponse);
                    return BadRequest(new { error = "Failed to delete from queue.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("nasol");
                return StatusCode(500, new { error = "An error occurred while deleting from queue.", details = ex.Message });
            }
        }
        [HttpGet("projects/{user_name}/{id}")]
        public async Task<IActionResult> ShowProjectDetails(string user_name, string id)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/projects/{user_name}/{id}.json";

            Console.WriteLine(url);

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(projectsData);
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve project details.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving projects.", details = ex.Message });
            }
        }
        [HttpGet("user/{user_name}/favorites")]
        public async Task<IActionResult> GetFavoritesForCurrentUser(string user_name)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/people/{user_name}/favorites/list.json";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve favorites.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving favorites.", details = ex.Message });
            }
        }
        [HttpGet("patterns/search")]
        public async Task<IActionResult> SearchPatternsDatabase([FromQuery] String? query, [FromQuery] int? page, [FromQuery] int? page_size)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = "";
            if(query=="")
            {
                var start = (page - 1) * page_size;
                var end = page * page_size;
                url = $"{RavelryApiUrl}/patterns.json?ids={query}&page={start}+{end}";
            }
            else
                url = $"{RavelryApiUrl}/patterns/search.json?query={query}&page={page}&page_size={page_size}";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve search results.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving favorites.", details = ex.Message });
            }
        }
        [HttpGet("user/{user_name}/queue")]
        public async Task<IActionResult> GetQueueForCurrentUser(string user_name)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/people/{user_name}/queue/list.json";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve favorites.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving favorites.", details = ex.Message });
            }
        }
        [HttpGet("user/{user_name}/friends_activity")]
        public async Task<IActionResult> GetFriendsActivity(string user_name)
        {
            string accessToken = _authService.getToken();

            Console.WriteLine("Access Token: " + accessToken);

            var url = $"{RavelryApiUrl}/people/{user_name}/friends/activity.json?activity_type_keys=added-project-photo+queued-pattern+added-favorite";

            accessToken = _authService.getToken();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var projectsData = await response.Content.ReadAsStringAsync();
                    return Ok(projectsData);
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    return BadRequest(new { error = "Failed to retrieve favorites.", details = errorResponse });
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving favorites.", details = ex.Message });
            }
        }
    }
}