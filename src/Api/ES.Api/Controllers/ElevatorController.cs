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

    [HttpPost("findnearest")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> FindNearestElevator([FromBody] ElevatorRequest request)
    {
        var validator = new ElevatorRequestValidator();
        if (!validator.Validate(request).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: validator.Validate(request).Errors);
        }

        var serviceResponse = await _serviceManager.ElevatorService.FindNearestElevator(request);
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException || serviceResponse.Data == null)
                return NoContent();

            return BadRequest(serviceResponse);
        }

        if (serviceResponse.Data == null)
            return NoContent();

        return Ok(serviceResponse.Data);
    }


    [HttpPost("dispatch")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> DispatchElevator([FromBody] ElevatorRequest request, int elevatorId)
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



}
