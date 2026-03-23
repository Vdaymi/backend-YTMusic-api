﻿using System.Net.Http.Json;
using YTMusicApi.Model.Integration;
using YTMusicApi.Shared.Optimization;

namespace YTMusicApi.Orchestrator.Integration
{
    public class OptimizerClient : IOptimizerClient
    {
        private readonly HttpClient _httpClient;

        public OptimizerClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<OptimizationResponse> OptimizePlaylistAsync(OptimizationSettingsDto request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/optimization/optimize", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OptimizationResponse>();

            return result ?? new OptimizationResponse
            {
                Success = false,
                ErrorMessage = "Optimizer returned an empty or invalid response."
            };
        }
    }
}
