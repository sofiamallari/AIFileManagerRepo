using AIFileManager.API.DTO;
using AIFileManager.DTO;
using AIFileManager.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Json;

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
        public async Task<AIAnalysisResultDto?> AnalyzeFolderAsync(List<FolderInfoDto> folders)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/thresholdLimit", folders);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AIAnalysisResultDto>(json);
        }
        // NEW: used by parallel batch system
        public async Task<DecisionDto> AnalyzeFileAsync(FileInfoDto file)
        {
            var payload = new
            {
                name = file.Name,
                sizeMB = file.SizeMB,
                modifiedDate = file.ModifiedDate,
                hash = file.Hash
            };

            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/analyze-file", payload);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<DecisionDto>();
        }
        public async Task<List<DecisionDto>?> AnalyzeBatchAsync(List<FileInfoDto> files)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/analyze-batch", files);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<DecisionDto>>();
        }
        public async Task<DecisionDto?> CheckLargeFileAsync(List<FileInfoDto> files)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/checkLargeFile", files);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DecisionDto>();
        }

        public async Task<ThresholdRespDto?> ThresholdLimitAsync(ThresholdReqDto req)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/thresholdLimit", req);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ThresholdRespDto>();
        }

        public async Task<DecisionDto?> AnalyzeSingleFileAsync(FileInfoDto file)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/analyze-file", file);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DecisionDto>();
        }

        public async Task<DecisionDto?> AnalyzeFolderAsync(FolderAnalysisReqDto req)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/analyzeFolder", req);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DecisionDto>();
        }

    }

}
