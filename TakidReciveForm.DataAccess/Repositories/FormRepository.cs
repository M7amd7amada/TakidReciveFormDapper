using System.Data;

using AutoMapper;

using Dapper;

using TakidReciveForm.DataAccess.Data;
using TakidReciveForm.Domain.Data;
using TakidReciveForm.Domain.DTOs.ReadDTOs;
using TakidReciveForm.Domain.DTOs.WriteDTOs;
using TakidReciveForm.Domain.Interfaces;
using TakidReciveForm.Domain.Models;
using TakidReciveForm.Domain.Services;

namespace TakidReciveForm.DataAccess.Repositories;

public class FormRepository : IFormRepository
{
    private readonly IDapperContext _context;
    private readonly IMapper _mapper;
    private readonly IAttachmentService _attachmentService;

    public FormRepository(IDapperContext context, IAttachmentService attachmentService, IMapper mapper)
    {
        _context = context;
        _attachmentService = attachmentService;
        _mapper = mapper;
    }

    public async Task<FormReadDto?> DeleteAsync(Guid id, string rootPath)
    {
        using IDbConnection connection = _context.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Form>(
            "GetImageNameAndDeleteForm",
            new { Id = id },
            commandType: CommandType.StoredProcedure);

        if (result is not null)
        {
            _attachmentService.DeleteFile(result.ImageName, rootPath);
        }
        else
        {
            throw new KeyNotFoundException("Form not found");
        }

        return _mapper.Map<FormReadDto>(result);
    }

    public PagedResult<FormReadDto> GetAll(int page, int pageSize = 5)
    {
        using IDbConnection connection = _context.CreateConnection();
        var forms = connection.Query<Form>(
            "GetAllForms",
            commandType: CommandType.StoredProcedure);

        var result = forms.GetPaged(page, pageSize);

        return _mapper.Map<PagedResult<FormReadDto>>(result);
    }

    public Task<FormReadDto> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<FormReadDto> InsertAsync(FormWriteDto formWriteDto, string rootPath)
    {
        await SaveImage(formWriteDto.ImageBase64, formWriteDto.ImageName, rootPath);

        using IDbConnection connection = _context.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("ReceiverName", formWriteDto.ReceiverName);
        parameters.Add("VechicleNumber", formWriteDto.VechicleNumber);
        parameters.Add("PickUpTime", formWriteDto.PickUpTime);
        parameters.Add("DeliveryTime", formWriteDto.DeliveryTime);
        parameters.Add("ImageBase64", formWriteDto.ImageBase64);
        parameters.Add("ImageName", formWriteDto.ImageName);
        parameters.Add("ReasonForReceipt", formWriteDto.ReasonForReceipt);
        parameters.Add("SignatureUponReceipt", formWriteDto.SignatureUponReceipt);
        parameters.Add("SignatureUponDelivery", formWriteDto.SignatureUponDelivery);

        parameters.Add("InsertedFormId", dbType: DbType.Guid, direction: ParameterDirection.Output);

        await connection.ExecuteAsync(
            "AddForm",
            parameters,
            commandType: CommandType.StoredProcedure
        );

        Guid insertedFormId = parameters.Get<Guid>("InsertedFormId");

        var result = await GetByIdAsync(insertedFormId);

        return _mapper.Map<FormReadDto>(result);
    }

    public Task<FormReadDto> UpdateAsync(Form form, string attachmentsPath)
    {
        throw new NotImplementedException();
    }
    private async Task SaveImage(string imageBase64, string imageName, string rootPath)
    {
        if (_attachmentService.IsBase64String(imageBase64))
        {
            byte[] bytes = _attachmentService.GetBase64Bytes(imageBase64);
            string path = _attachmentService.GetFilePath(imageName, rootPath);
            await _attachmentService.SaveFileAsync(bytes, path);
        }
    }
}