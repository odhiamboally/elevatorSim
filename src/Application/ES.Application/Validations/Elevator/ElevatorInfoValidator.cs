using ES.Application.Dtos.Elevator;

using FluentValidation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Validations.Elevator;

public class ElevatorInfoValidator : AbstractValidator<ElevatorInfo>
{
    public ElevatorInfoValidator()
    {
        // Validate Id (assuming it must be a positive integer)
        RuleFor(elevator => elevator.Id)
            .GreaterThan(0).WithMessage("Elevator ID must be a positive number.");

        // Validate CurrentLoad (must be between 0 and MaxCapacity)
        RuleFor(elevator => elevator.CurrentLoad)
            .InclusiveBetween(0, 10)
            .WithMessage($"Current load must be between 0 and {10}.");

        // Validate CurrentFloor (if there is a known range of valid floors, validate it here)
        RuleFor(elevator => elevator.CurrentFloor)
            .GreaterThanOrEqualTo(0).WithMessage("Current floor must be 0 or higher.");

        // Validate Status (ensure it's a valid enum value)
        RuleFor(elevator => elevator.Status)
            .IsInEnum().WithMessage("Invalid elevator status.");

        // Validate Direction (ensure it's a valid enum value)
        RuleFor(elevator => elevator.Direction)
            .IsInEnum().WithMessage("Invalid elevator direction.");

        // Validate RequestQueue (optional - if you have rules for requests, apply them here)
        RuleForEach(elevator => elevator.RequestQueue)
            .SetValidator(new ElevatorRequestValidator());
    }
}

