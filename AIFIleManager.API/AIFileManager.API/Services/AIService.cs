using AIFileManager.DTO;
using AIFileManager.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AIFileManager.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:8000"; // FastAPI endpoint

        public AIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AIAnalysisResultDto?> AnalyzeFilesAsync(List<FileInfoDto> files)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/checkLargeFile", files);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AIAnalysisResultDto>(json);
        }

        public async Task<AIAnalysisResultDto?> AnalyzeFolderAsync(List<FolderInfoDto> folders)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/thresholdLimit", folders);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AIAnalysisResultDto>(json);
        }

        public async Task<AIAnalysisResultDto?> OptimizeAsync(object metadata)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/optimize", metadata);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AIAnalysisResultDto>(json);
        }
    }
}
