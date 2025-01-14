using ES.Application.Abstractions.Interfaces;
using ES.Application.Dtos.Elevator;
using ES.Application.Validations.Elevator;
using ES.Shared.Exceptions;

using FluentValidation;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ES.Api.Controllers;



[Route("api/[controller]")]
[ApiController]
public class FloorController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public FloorController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;

    }

    [HttpPost("addrequesttofloorqueue/{floorNumber:int}")]
    public async Task<ActionResult> AddRequestToFloorQueue([FromBody] RequestInfo request, int floorNumber) 
    {
        var elevatorRequestValidator = new ElevatorRequestValidator();
        if (!elevatorRequestValidator.Validate(request).IsValid)
        {
            throw new ValidationException("Request Object is Invalid", errors: elevatorRequestValidator.Validate(request).Errors);
        }

        var serviceResponse = await _serviceManager.FloorQueueManager.AddRequestToFloor(floorNumber, request);
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
        var serviceResponse = await _serviceManager.FloorQueueManager.ProcessAllFloorQueues();
        if (!serviceResponse.Successful)
        {
            if (serviceResponse.Exception is NoContentException)
                return NoContent();

            return BadRequest();
        }

        return Ok(serviceResponse.Data);
    }
}
