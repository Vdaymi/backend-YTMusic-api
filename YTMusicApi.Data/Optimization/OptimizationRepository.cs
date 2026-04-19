﻿using AutoMapper;
using YTMusicApi.Data.MessageBroker;
using YTMusicApi.Model.Optimization;
using YTMusicApi.Model.MessageBroker;

namespace YTMusicApi.Data.Optimization
{
    public class OptimizationRepository : IOptimizationRepository
    {
        private readonly SqlDbContext _context;
        private readonly IMapper _mapper;

        public OptimizationRepository(SqlDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task CreateTaskAndOutboxMessageAsync(OptimizationTaskDto taskDto, OutboxMessageDto outboxMessageDto)
        {
            var task = _mapper.Map<OptimizationTaskDao>(taskDto);
            var outboxMessage = _mapper.Map<OutboxMessageDao>(outboxMessageDto);

            await _context.OptimizationTasks.AddAsync(task);
            await _context.OutboxMessages.AddAsync(outboxMessage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskStatusAsync(Guid taskId, OptimizationTaskStatus status, string? errorMessage = null)
        {
            var task = await _context.OptimizationTasks.FindAsync(taskId);
            if (task != null)
            {
                task.Status = status;
                if (errorMessage != null) task.ErrorMessage = errorMessage;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<OptimizationTaskDto?> GetTaskByIdAsync(Guid taskId)
        {
            var taskDao = await _context.OptimizationTasks.FindAsync(taskId);
            return taskDao == null ? null : _mapper.Map<OptimizationTaskDto>(taskDao);
        }
    }
}