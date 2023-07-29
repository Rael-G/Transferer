﻿using Api.Models;

namespace Api.Data.Interfaces
{
    public interface IArchiveRepository
    {
        Task<Archive> SaveAsync(Archive archive);
        Task<Archive> GetByIdAsync(int id);
    }
}
