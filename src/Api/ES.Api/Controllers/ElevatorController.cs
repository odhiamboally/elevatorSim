using ES.Application.Abstractions.Interfaces;
using ES.Application.Dtos.Elevator;
using ES.Application.Validations.Elevator;
using ES.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers;



[Route("api/[controller]")]
[ApiController]
public class ElevatorController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IServiceManager _serviceManager;

    public ElevatorController(IConfiguration configuration, IServiceManager serviceManager)
    {
          _configuration = configuration;
        _serviceManager = serviceManager;
    }



    [HttpPost("completerequest")]
    public async Task<ActionResult> CompleteRequest([FromBody] CompleteRequest request) //
    {
        if (request == null)
            return BadRequest("Invalid request.");

        var serviceResponse = await _serviceManager.ElevatorService.CompleteRequest(request);
        return Ok(serviceResponse.Data);
    }

    [HttpPost("dispatch")]
    public async Task<ActionResult> Dispatch([FromBody] RequestInfo request, int elevatorId)
    {
        var validator = new ElevatorRequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: validator.Validate(request).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.DispatchElevator(elevatorId, request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException || !serviceResponse.Data)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        return Ok(serviceResponse.Data);
    }

    [HttpPost("dispatchelevator")]
    public async Task<ActionResult> DispatchElevator([FromBody] DispatchElevatorRequest request)
    {
        var elevatorRequestValidator = new ElevatorRequestValidator();
        if (!elevatorRequestValidator.Validate(request.ElevatorRequest!).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: elevatorRequestValidator.Validate(request.ElevatorRequest!).Errors);
        }

        var elevatorInfoValidator = new ElevatorInfoValidator();
        if (!elevatorInfoValidator.Validate(request.ElevatorInfo!).IsValid)
        {
            throw new ValidationException("Request Object() is Invalid", errors: elevatorInfoValidator.Validate(request.ElevatorInfo!).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.DispatchElevator(request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            return BadRequest();
        }

        return Ok(serviceResponse.Data);
    }

    [HttpGet("enqueuerequeststoelevators")]
    public async Task<ActionResult> EnqueueRequestsToElevators()
    {
        var serviceResponse = await _serviceManager.ElevatorService.EnqueueRequestsToElevators();
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            return BadRequest();
        }

        return Ok(serviceResponse.Data);
    }

    [HttpPost("find")]
    public async Task<ActionResult> Find([FromBody] RequestInfo request)
    {
        var validator = new ElevatorRequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: validator.Validate(request).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.FindElevator(request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        if (serviceResponse.Data == null)
            return NoContent();

        return Ok(serviceResponse.Data);
    }

    [HttpGet("elevatorstate/{elevatorId:int}")]
    public async Task<ActionResult<ElevatorInfo>> ElevatorState(int elevatorId, ElevatorInfo elevatorInfo)
    {
        var serviceResponse = await _serviceManager.ElevatorStateManager.FetchElevatorStateAsync(elevatorId, elevatorInfo);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            if (serviceResponse.Data == null)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        if (serviceResponse.Data == null)
            return NoContent();

        return Ok(serviceResponse.Data);
    }

    [HttpGet("elevatorstates")]
    public async Task<ActionResult<List<ElevatorInfo>>> ElevatorStates()
    {
        var serviceResponse = await _serviceManager.ElevatorStateManager.FetchElevatorStatesAsync();
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            if (serviceResponse.Data == null || !serviceResponse.Data.Any())
                return NoContent();

            return BadRequest(serviceResponse);
        }

        if (!serviceResponse.Data!.Any())
            return NoContent();

        return Ok(serviceResponse.Data);
    }

    [HttpPost("load")]
    public async Task<ActionResult> LoadElevator([FromBody] LoadElevatorRequest request)
    {
        var elevatorRequestValidator = new ElevatorRequestValidator();
        if (!elevatorRequestValidator.Validate(request.ElevatorRequest!).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: elevatorRequestValidator.Validate(request.ElevatorRequest!).Errors);
        }

        var elevatorInfoValidator = new ElevatorInfoValidator();
        if (!elevatorInfoValidator.Validate(request.ElevatorInfo!).IsValid)
        {
            throw new ValidationException("Request Object() is Invalid", errors: elevatorInfoValidator.Validate(request.ElevatorInfo!).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.LoadElevator(request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            return BadRequest();
        }

        return Ok(serviceResponse.Data);
    }

    [HttpPost("offload")]
    public async Task<ActionResult> Offload([FromBody] OffloadRequest request)
    {
        var elevatorRequestValidator = new ElevatorRequestValidator();
        if (!elevatorRequestValidator.Validate(request.ElevatorRequest!).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: elevatorRequestValidator.Validate(request.ElevatorRequest!).Errors);
        }

        var elevatorInfoValidator = new ElevatorInfoValidator();
        if (!elevatorInfoValidator.Validate(request.ElevatorInfo!).IsValid)
        {
            throw new ValidationException("Request Object() is Invalid", errors: elevatorInfoValidator.Validate(request.ElevatorInfo!).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.OffloadElevator(request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException || serviceResponse.Data == null)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        return Ok(serviceResponse.Data);
    }

    [HttpPost("updateelevatorstate")]
    public async Task<ActionResult> UpdateElevatorState([FromBody] ElevatorInfo elevatorInfo)
    {

        var elevatorInfoValidator = new ElevatorInfoValidator();
        if (!elevatorInfoValidator.Validate(elevatorInfo).IsValid)
        {
            throw new ValidationException("Request Object() is Invalid", errors: elevatorInfoValidator.Validate(elevatorInfo).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorStateManager.UpdateElevatorStateAsync(elevatorInfo);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException || serviceResponse.Data == null)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        return Ok(serviceResponse.Data);
    }



}
