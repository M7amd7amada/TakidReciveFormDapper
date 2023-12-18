using TakidReciveForm.Domain.Data;
using TakidReciveForm.Domain.DTOs.ReadDTOs;
using TakidReciveForm.Domain.DTOs.WriteDTOs;
using TakidReciveForm.Domain.Models;

namespace TakidReciveForm.Domain.Interfaces;

public interface IFormRepository
{
    public PagedResult<FormReadDto> GetAll(int page, int pageSize);
    public Task<FormReadDto> GetByIdAsync(Guid id);
    public Task<FormReadDto?> DeleteAsync(Guid id, string rootPath);
    public Task<FormReadDto> InsertAsync(FormWriteDto form, string rootPath);
    public Task<FormReadDto> UpdateAsync(Form form, string rootPath);
}