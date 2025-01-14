using ES.Shared.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.IO.Pipelines;

using Web.Client.Blazor.Dtos;
using Web.Client.Blazor.Enums;
using Web.Client.Blazor.Utilities.Api;
using Web.Client.Blazor.Utilities.SignalR;

namespace Web.Client.Blazor.Components.Pages;

public partial class Elevator : IAsyncDisposable
{
    private int ElevatorId;
    private int FromFloor;
    private int ToFloor;
    private int PeopleCount;
    private int Direction;

    private RequestInfo? elevatorRequest;
    private List<ElevatorInfo> elevatorStates = new();
    private List<FloorInfo> Floors = new();

    private bool requestsEnqueuedToElevators = false;
    private bool isFormVisible = false;
    private bool isLoading = false;
    private string errorMessage = string.Empty;
    private long responseTime;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private ISignalRService SignalRService { get; set; } = default!;

    [Inject]
    private IApiClient ApiClient { get; set; } = default!;




    protected override async Task OnInitializedAsync()
    {
        for (int i = 1; i <= 10; i++)
        {
            Floors.Add(new FloorInfo { Id = i });
        }

        SignalRService.ElevatorStateReceived += OnElevatorStateReceived;
        SignalRService.ElevatorStatesReceived += OnElevatorStatesReceived;

        await SignalRService.StartAsync();

        if (!elevatorStates.Any())
        {
            // Process all floor queues and enqueue requests to elevators
            await EnqueueRequestsToElevators();

            await LoadElevatorStatesAsync();
        }
    }

    private async Task LoadElevatorStatesAsync()
    {
        try
        {
            var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:GetElevatorStates"];
            if (string.IsNullOrEmpty(apiEndPoint))
                throw new ApiEndPointException("API endpoint is not configured.");

            elevatorStates = await ApiClient.FetchElevatorData(apiEndPoint);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
    private async Task EnqueueRequestsToElevators()
    {
        try
        {
            var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:EnqueueRequestsToElevators"];
            if (string.IsNullOrEmpty(apiEndPoint))
                throw new ApiEndPointException("API endpoint is not configured.");

            requestsEnqueuedToElevators = await ApiClient.EnqueueRequestsToElevators(apiEndPoint);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private void OnElevatorStateReceived(int elevatorId, ElevatorInfo info)
    {
        InvokeAsync(() =>
        {
            var existingElevator = elevatorStates.Where(ev=> ev != null).FirstOrDefault(e => e.Id == elevatorId);
            if (existingElevator != null)
            {
                var index = elevatorStates.IndexOf(existingElevator);
                elevatorStates[index] = info;
                //elevatorStates.RemoveAll(e => e.Id == elevatorId);
            }
            else
            {
                elevatorStates.Add(info);
            }

            StateHasChanged();

        });
    }

    private void OnElevatorStatesReceived(List<KeyValuePair<int, ElevatorInfo>> elevatorInfos)
    {
        // Ensure that StateHasChanged is invoked on the correct Dispatcher thread
        InvokeAsync(() =>
        {
            elevatorStates = elevatorInfos.Select(e => e.Value).ToList();
            StateHasChanged();
        });
    }

    private void OpenRequestForm(int floor, int direction)
    {
        FromFloor = floor;
        Direction = direction;
        elevatorRequest = new RequestInfo(ElevatorId, FromFloor, ToFloor, PeopleCount, direction);
        isFormVisible = true;
    }

    private async Task HandleValidSubmit_(EditContext context)
    {
        try
        {
            Queue<RequestInfo> queue = [];
            RequestInfo? completedRequest = null;

            var stopwatch = Stopwatch.StartNew();

            //elevatorRequest = new ElevatorRequest(ElevatorId, FromFloor, ToFloor, PeopleCount, Direction);

            var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:Find"];
            var apiResponse = await ApiClient.FindElevator(elevatorRequest!, apiEndPoint!);

            var availableSpace = apiResponse.Capacity - apiResponse.CurrentLoad;
            if (elevatorRequest!.PeopleCount > availableSpace)
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", "Info", $"Number of passengers requested for the elevator exceeds capacity. \n " +
                    $"Only {apiResponse.Capacity - apiResponse.CurrentLoad} passengers will baord", "info");

                elevatorRequest.UpdatePeopleCount(availableSpace);
            }

            if (apiResponse != null)
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", "Success", $"Elevator {apiResponse.Id} has been assigned and is being dispatched!", "success");
                elevatorRequest.UpdateElevatorId(apiResponse.Id);
                
                var addToFloorQueueEndpoint = Configuration["AppSettings:EndPoints:Floor:AddRequestToQueue"];
                if (string.IsNullOrWhiteSpace(addToFloorQueueEndpoint))
                {
                    throw new NotFoundException($"Endpoint not configured. {addToFloorQueueEndpoint}");
                }

                var requestId = await ApiClient.AddRequestToFloorQueue(elevatorRequest, FromFloor, addToFloorQueueEndpoint);
                if (requestId > 0 && requestId == elevatorRequest.Id)
                {
                    await JSRuntime.InvokeVoidAsync("Swal.fire",
                                "Success",
                                $"Request with Id: {elevatorRequest.Id} added to Request Queue for floor {FromFloor}.", "success");
                }

                //Dispatch Elevator

                DispatchElevatorRequest dispatchElevatorRequest = new()
                {
                    ElevatorRequest = elevatorRequest,
                    ElevatorInfo = apiResponse
                    
                };

                var dispatchEndPoint = Configuration["AppSettings:EndPoints:Elevator:DispatchElevator"];
                var dispatchResponse = await ApiClient.DispatchElevator(dispatchElevatorRequest, dispatchEndPoint!); 
                if (dispatchResponse != null)
                {
                    apiResponse = dispatchResponse;
                    await SimulateElevatorMovement(apiResponse, dispatchElevatorRequest.ElevatorRequest.FromFloor); // To Request Floor

                    //Load Elevator

                    LoadElevatorRequest loadElevatorRequest = new()
                    {
                        ElevatorRequest = elevatorRequest,
                        ElevatorInfo = apiResponse,
                        PeopleCount =elevatorRequest.PeopleCount,
                        RequestId = elevatorRequest.Id

                    };

                    var loadEndPoint = Configuration["AppSettings:EndPoints:Elevator:LoadElevator"];
                    var loadResponse = await ApiClient.LoadElevator(loadElevatorRequest, loadEndPoint!);
                    if (loadResponse != null)
                    {
                        apiResponse = loadResponse;
                        var elevatorForOffload = await SimulateElevatorMovement(apiResponse, dispatchElevatorRequest.ElevatorRequest.ToFloor); // To Destination Floor

                        //Offload Elevator

                        OffloadRequest offloadRequest = new()
                        {
                            ElevatorInfo = elevatorForOffload,
                            ElevatorRequest = elevatorRequest,
                        };

                        var offloadEndpoit = Configuration["AppSettings:EndPoints:Elevator:OffLoadElevator"];
                        if (string.IsNullOrWhiteSpace(offloadEndpoit))
                        {
                            throw new NotFoundException($"Endpoint not configured. {offloadEndpoit}");
                        }

                        var offloadResult = await ApiClient.OffloadElevator(offloadRequest, offloadEndpoit);
                        if (offloadEndpoit != null)
                        {
                            await JSRuntime.InvokeVoidAsync("Swal.fire", 
                                "Success", 
                                $"{offloadRequest.ElevatorRequest.PeopleCount} passengers offloaded from Elevator {offloadResult.Id} at floor {offloadRequest.ElevatorRequest.ToFloor}", "success");

                            apiResponse = offloadResult;
                            CompleteRequest completeRequest = new()
                            {
                                ElevatorInfo = apiResponse,
                                ElevatorRequest = elevatorRequest,
                            };

                            var completeRequestEndPoint = Configuration["AppSettings:EndPoints:Elevator:CompleteRequest"];
                            if (string.IsNullOrWhiteSpace(completeRequestEndPoint))
                            {
                                throw new NotFoundException($"Endpoint not configured. {completeRequestEndPoint}");
                            }

                            var completeRequestResult = await ApiClient.CompleteRequest(completeRequest, completeRequestEndPoint);
                            if (completeRequestResult != null)
                            {
                                apiResponse = completeRequestResult;
                                queue.Enqueue(elevatorRequest);
                                completedRequest = elevatorRequest;
                            }

                        }
                        else
                        {
                            await JSRuntime.InvokeVoidAsync("Swal.fire", new
                            {
                                title = "Problem",
                                text = $"Elevator {loadResponse!.Id} was not loaded.",
                                icon = "error",
                                timer = 3000, 
                                position = "center", 
                                showConfirmButton = true,
                                confirmButtonColor = "#3085d6"
                            });
                        }

                    }
                    else
                    {
                        await JSRuntime.InvokeVoidAsync("Swal.fire", new
                        {
                            title = "Problem",
                            text = $"Elevator {loadResponse!.Id} was not loaded.",
                            icon = "error",
                            timer = 3000, 
                            position = "center", 
                            showConfirmButton = true,
                            confirmButtonColor = "#3085d6"
                        });
                    }

                }
                else
                {
                    await JSRuntime.InvokeVoidAsync("Swal.fire", new
                    {
                        title = "Problem",
                        text = $"Elevator {dispatchResponse!.Id} was not dispatched.",
                        icon = "error",
                        timer = 3000, 
                        position = "center", 
                        showConfirmButton = true,
                        confirmButtonColor = "#3085d6"
                    });
                }

                OnElevatorStateReceived(apiResponse.Id, apiResponse);
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", new
                {
                    title = "Info",
                    text = $"Elevators might be busy at the moment. Please wait...",
                    icon = "info",
                    timer = 3000, 
                    position = "center", 
                    showConfirmButton = true,
                    confirmButtonColor = "#3085d6"
                });
            }

            stopwatch.Stop();
            responseTime = stopwatch.ElapsedMilliseconds;

            isFormVisible = false;

            FromFloor = 0;
            Direction = 0;
            PeopleCount = 0;
            elevatorRequest = new RequestInfo(0,0,0,0,0);

            StateHasChanged();

            
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("Swal.fire", "Error", ex.Message, "error");
            throw;
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleValidSubmit(EditContext context)
    {
        try
        {
            InitializeRequest();
            var apiResponse = await FindElevator();
            if (apiResponse == null) return;

            await AssignAndDispatchElevator(apiResponse);
        }
        catch (Exception ex)
        {
            await ShowSwalError("Error", ex.Message);
            throw;
        }
        finally
        {
            isLoading = false;
        }
    }

    private void InitializeRequest()
    {
        elevatorRequest = new RequestInfo(ElevatorId, FromFloor, ToFloor, PeopleCount, Direction);
    }

    private async Task<ElevatorInfo?> FindElevator()
    {
        var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:Find"];
        var apiResponse = await ApiClient.FindElevator(elevatorRequest!, apiEndPoint!);

        if (apiResponse == null)
        {
            await ShowSwalInfo("Info", "Elevators might be busy at the moment. Please wait...");
            return null;
        }

        if (elevatorRequest!.PeopleCount > apiResponse.Capacity - apiResponse.CurrentLoad)
        {
            var availableSpace = apiResponse.Capacity - apiResponse.CurrentLoad;
            await ShowSwalInfo("Info", $"Requested passengers exceed capacity. Only {availableSpace} will board.");
            elevatorRequest.UpdatePeopleCount(availableSpace);
        }

        await ShowSwalSuccess("Success", $"Elevator {apiResponse.Id} has been assigned and is being dispatched!");
        elevatorRequest.UpdateElevatorId(apiResponse.Id);

        return apiResponse;
    }

    private async Task AssignAndDispatchElevator(ElevatorInfo apiResponse)
    {
        var requestId = await AddRequestToQueue(elevatorRequest!);
        if (requestId != elevatorRequest!.Id) return;

        var dispatchResult = await DispatchElevator(apiResponse);
        if (dispatchResult == null) return;

        var loadResult = await LoadElevator(dispatchResult);
        if (loadResult == null) return;

        var offloadResult = await OffloadElevator(loadResult);
        if (offloadResult == null) return;

        await CompleteElevatorRequest(offloadResult);
        ResetForm();
    }

    private async Task<int> AddRequestToQueue(RequestInfo elevatorRequest)
    {
        var endpoint = Configuration["AppSettings:EndPoints:Floor:AddRequestToQueue"];
        ValidateEndpoint(endpoint, nameof(AddRequestToQueue));
        return await ApiClient.AddRequestToFloorQueue(elevatorRequest!, FromFloor, endpoint!);
    }

    private async Task<ElevatorInfo?> DispatchElevator(ElevatorInfo apiResponse)
    {
        var endpoint = Configuration["AppSettings:EndPoints:Elevator:DispatchElevator"];
        ValidateEndpoint(endpoint, nameof(DispatchElevator));

        var dispatchRequest = new DispatchElevatorRequest
        {
            ElevatorRequest = elevatorRequest,
            ElevatorInfo = apiResponse
        };

        var dispatchResponse = await ApiClient.DispatchElevator(dispatchRequest, endpoint!);
        if (dispatchResponse == null)
        {
            await ShowSwalError("Problem", $"Elevator {apiResponse.Id} was not dispatched.");
            return null;
        }

        await SimulateElevatorMovement(dispatchResponse, elevatorRequest!.FromFloor);
        return dispatchResponse;
    }

    private async Task<ElevatorInfo?> LoadElevator(ElevatorInfo apiResponse)
    {
        var endpoint = Configuration["AppSettings:EndPoints:Elevator:LoadElevator"];
        ValidateEndpoint(endpoint, nameof(LoadElevator));

        var loadRequest = new LoadElevatorRequest
        {
            ElevatorRequest = elevatorRequest,
            ElevatorInfo = apiResponse,
            PeopleCount = elevatorRequest!.PeopleCount,
            RequestId = elevatorRequest.Id
        };

        var loadResponse = await ApiClient.LoadElevator(loadRequest, endpoint!);
        if (loadResponse == null)
        {
            await ShowSwalError("Problem", $"Elevator {apiResponse.Id} was not loaded.");
            return null;
        }

        await SimulateElevatorMovement(loadResponse, elevatorRequest.ToFloor);
        return loadResponse;
    }

    private async Task<ElevatorInfo?> OffloadElevator(ElevatorInfo apiResponse)
    {
        var endpoint = Configuration["AppSettings:EndPoints:Elevator:OffLoadElevator"];
        ValidateEndpoint(endpoint, nameof(OffloadElevator));

        var offloadRequest = new OffloadRequest
        {
            ElevatorInfo = apiResponse,
            ElevatorRequest = elevatorRequest
        };

        var offloadResponse = await ApiClient.OffloadElevator(offloadRequest, endpoint!);
        if (offloadResponse == null)
        {
            await ShowSwalError("Problem", $"Elevator {apiResponse.Id} was not offloaded.");
            return null;
        }

        await ShowSwalSuccess("Success", $"{offloadRequest.ElevatorRequest!.PeopleCount} passengers offloaded at floor {offloadRequest.ElevatorRequest.ToFloor}.");
        return offloadResponse;
    }

    private async Task CompleteElevatorRequest(ElevatorInfo apiResponse)
    {
        var endpoint = Configuration["AppSettings:EndPoints:Elevator:CompleteRequest"];
        ValidateEndpoint(endpoint, nameof(CompleteElevatorRequest));

        var completeRequest = new CompleteRequest
        {
            ElevatorInfo = apiResponse,
            ElevatorRequest = elevatorRequest
        };

        var completeResult = await ApiClient.CompleteRequest(completeRequest, endpoint!);
        if (completeResult != null)
        {
            await ShowSwalSuccess("Success", "Elevator request completed successfully.");
        }
    }

    private void ValidateEndpoint(string? endpoint, string methodName)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new NotFoundException($"Endpoint not configured for {methodName}.");
        }
    }

    private async Task ShowSwalInfo(string title, string message) =>
        await JSRuntime.InvokeVoidAsync("Swal.fire", title, message, "info");

    private async Task ShowSwalSuccess(string title, string message) =>
        await JSRuntime.InvokeVoidAsync("Swal.fire", title, message, "success");

    private async Task ShowSwalError(string title, string message) =>
        await JSRuntime.InvokeVoidAsync("Swal.fire", title, message, "error");

    private void ResetForm()
    {
        FromFloor = 0;
        ToFloor = 0;
        Direction = 0;
        PeopleCount = 0;
        isFormVisible = false;
        StateHasChanged();
    }


    public async ValueTask DisposeAsync()
    {
        // Unsubscribe from SignalR events
        SignalRService.ElevatorStateReceived -= OnElevatorStateReceived;
        SignalRService.ElevatorStatesReceived -= OnElevatorStatesReceived;

        // Stop the SignalR connection
        await SignalRService.StopAsync();

        
    }

    public async Task<Queue<RequestInfo>> ProcessRequests(ElevatorInfo elevatorInfo)
    {
        try
        {
            Queue<RequestInfo> queue = [];
            RequestInfo? completedRequest = null;
            while (elevatorInfo.RequestQueue.Any())
            {
                foreach (var request in elevatorInfo.RequestQueue)
                {
                    await SimulateElevatorMovement(elevatorInfo, request.ToFloor); // To Destination Floor

                    OffloadRequest offloadRequest = new()
                    {
                        ElevatorInfo = elevatorInfo,
                        ElevatorRequest = request,
                    };

                    // Offload passengers
                    var offloadEndpoit = Configuration["AppSettings:EndPoints:Elevator:OffLoadElevator"];
                    if (string.IsNullOrWhiteSpace(offloadEndpoit))
                    {
                        throw new NotFoundException($"Endpoint not configured. {offloadEndpoit}");
                    }

                    var offloadResult = await ApiClient.OffloadElevator(offloadRequest, offloadEndpoit);
                    if (offloadEndpoit != null)
                    {
                        await JSRuntime.InvokeVoidAsync("Swal.fire", "Success", $"{request.PeopleCount} passengers offloaded from Elevator {elevatorInfo.Id} at floor {request.ToFloor}", "success");
                    }

                    CompleteRequest completeRequest = new()
                    {
                        ElevatorInfo = elevatorInfo,
                        ElevatorRequest = request,
                    };

                    var completeRequestEndPoint = Configuration["AppSettings:EndPoints:Elevator:CompleteRequest"];
                    if (string.IsNullOrWhiteSpace(completeRequestEndPoint))
                    {
                        throw new NotFoundException($"Endpoint not configured. {completeRequestEndPoint}");
                    }

                    var completeRequestResult = await ApiClient.CompleteRequest(completeRequest, completeRequestEndPoint);
                    if (completeRequestResult != null)
                    {
                        queue.Enqueue(request);
                        completedRequest = request;
                    }

                }

                elevatorInfo.DequeueRequest(completedRequest!);

            }

            return queue;
        }
        catch (Exception)
        {

            throw;
        }
    }

    private async Task<ElevatorInfo> SimulateElevatorMovement(ElevatorInfo elevator, int targetFloor)
    {
        int step = Math.Sign(targetFloor - elevator.CurrentFloor);
        string direction = targetFloor > elevator.CurrentFloor ? "Up" : "Down";

        while (elevator.CurrentFloor < targetFloor)
        {
            // Update current floor based on direction
            elevator.UpdateCurrentFloor(step);
            step++;

            //ToDo: Should Be A Background Job - Update Status
            var updateStateEndpoint = Configuration["AppSettings:EndPoints:Elevator:UpdateElevatorState"];
            if (string.IsNullOrWhiteSpace(updateStateEndpoint))
            {
                throw new NotFoundException($"Endpoint not configured. {nameof(updateStateEndpoint)}");
            }

            await ApiClient.UpdateElevatorStateAsync(elevator, updateStateEndpoint);


            await Task.Delay(1000);

            if (elevator.CurrentFloor < targetFloor)
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", "Success", $"Elevator {elevator.Id} at floor {elevator.CurrentFloor}, moving {direction} to floor {targetFloor}", "success");
            }

        }

        while (elevator.CurrentFloor > targetFloor)
        {
            step = elevator.CurrentFloor - 1;
            elevator.UpdateCurrentFloor(step);

            //ToDo: Should Be A Background Job - Update Status
            var updateStateEndpoint = Configuration["AppSettings:EndPoints:Elevator:UpdateElevatorState"];
            if (string.IsNullOrWhiteSpace(updateStateEndpoint))
            {
                throw new NotFoundException($"Endpoint not configured. {nameof(updateStateEndpoint)}");
            }

            await ApiClient.UpdateElevatorStateAsync(elevator, updateStateEndpoint);

            await Task.Delay(1000);

            if (elevator.CurrentFloor > targetFloor)
            {
                await JSRuntime.InvokeVoidAsync("Swal.fire", "Success", $"Elevator {elevator.Id} at floor {elevator.CurrentFloor}, moving {direction} to floor {targetFloor}", "success");
            }

        }

        await JSRuntime.InvokeVoidAsync("Swal.fire",
        "Success",
        $"Elevator {elevator.Id} has reached floor {targetFloor}.",
        "success");

        return elevator;
    }



}
