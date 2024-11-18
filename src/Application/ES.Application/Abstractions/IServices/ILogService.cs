using ES.Application.Dtos.Common;
using ES.Application.Dtos.Logging;



namespace ES.Application.Abstractions.IServices;

public interface ILogService
{
    Task<Response<LogResponse>> CreateAsync(CreateLogRequest request);
    Task<Response<LogResponse>> DeleteAsync(int id);
    Task<Response<LogResponse>> FindByIdAsync(int id);
    

}
