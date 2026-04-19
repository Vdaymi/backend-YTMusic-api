﻿using YTMusicApi.Model.MessageBroker;

namespace YTMusicApi.Model.Optimization
{
    public interface IOptimizationRepository
    {
        Task CreateTaskAndOutboxMessageAsync(OptimizationTaskDto task, OutboxMessageDto outboxMessage);
        Task UpdateTaskStatusAsync(Guid taskId, OptimizationTaskStatus status, string? errorMessage = null);
        Task<OptimizationTaskDto?> GetTaskByIdAsync(Guid taskId);
    }
}