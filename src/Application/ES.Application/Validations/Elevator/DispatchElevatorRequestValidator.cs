using ES.Application.Dtos.Elevator;

using FluentValidation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Validations.Elevator;
public class DispatchElevatorRequestValidator : AbstractValidator<DispatchElevatorRequest>
{
    public DispatchElevatorRequestValidator()
    {
        RuleFor(x => x.ElevatorRequest!.FromFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("From floor must be greater than or equal to 0.");

        RuleFor(x => x.ElevatorRequest!.ToFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("To floor must be greater than or equal to 0.");

        RuleFor(x => x.ElevatorRequest!.PeopleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("People count must be greater than or equal to 0.");

        RuleFor(x => x.ElevatorRequest!.Direction)
            .NotNull()
            .WithMessage("Direction is required.");



        RuleFor(x => x.ElevatorInfo!.Id)
            .NotNull()
            .WithMessage("{PropertyName} is required.");

        RuleFor(x => x.ElevatorInfo!.Capacity)
            .GreaterThanOrEqualTo(10)
            .WithMessage("To floor must be greater than or equal to 10.");

        RuleFor(x => x.ElevatorInfo!.Status)
            .NotNull()
            .WithMessage("{PropertyName} is required.");

        RuleFor(x => x.ElevatorInfo!.Direction)
            .NotNull()
            .WithMessage("{PropertyName} is required.");

        RuleFor(x => x.ElevatorInfo!.RequestQueue)
            .Empty()
            .WithMessage("{PropertyName} must not be empty.");

    }
}
