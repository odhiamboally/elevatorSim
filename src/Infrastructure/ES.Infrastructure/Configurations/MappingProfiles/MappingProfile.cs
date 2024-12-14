using AutoMapper;

using ES.Application.Dtos.Elevator;
using ES.Application.Dtos.Floor;
using ES.Domain.Entities;
using ES.Domain.Enums;


namespace ES.Infrastructure.Configurations.MappingProfiles;
public class MappingProfile : Profile
{
    public MappingProfile()
    {

    }

    public static Request MapToEntity(RequestInfo requestInfo)
    {
        return new Request
        {
            Id = requestInfo.Id,
            ElevatorId = requestInfo.ElevatorId,
            FromFloor = requestInfo.FromFloor,
            ToFloor = requestInfo.ToFloor,
            PeopleCount = requestInfo.PeopleCount,
            Direction = requestInfo.Direction
        };
    }

    public static RequestInfo MapToDto(Request request)
    {
        return new RequestInfo(
            elevatorId: request.ElevatorId,
            fromFloor: request.FromFloor,
            toFloor: request.ToFloor,
            peopleCount: request.PeopleCount,
            direction: request.Direction)
        {
            Id = request.Id
        };
    }

    // Sample mapping method to convert Queue<int> to Queue<ElevatorRequest>.
    private Queue<RequestInfo> MapRequestQueue(Queue<int> requestQueueIds, Dictionary<int, RequestInfo> requestLookup)
    {
        // Create a new queue to hold the mapped ElevatorRequest objects.
        var mappedQueue = new Queue<RequestInfo>();

        // Iterate over the requestQueueIds and fetch corresponding ElevatorRequest objects.
        foreach (var id in requestQueueIds)
        {
            if (requestLookup.TryGetValue(id, out var elevatorRequest))
            {
                mappedQueue.Enqueue(elevatorRequest);
            }
            else
            {
                throw new KeyNotFoundException($"ElevatorRequest with ID {id} not found.");
            }
        }

        return mappedQueue;
    }

}
