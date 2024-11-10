using ES.Application.Dtos.Elevator;
using FluentValidation;

namespace ES.Application.Validations.Elevator;

public class ElevatorRequestValidator : AbstractValidator<ElevatorRequest>
{
    public ElevatorRequestValidator()
    {
        RuleFor(x => x.FromFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("From floor must be greater than or equal to 0.");

        RuleFor(x => x.ToFloor)
            .GreaterThanOrEqualTo(0)
            .WithMessage("To floor must be greater than or equal to 0.");

        RuleFor(x => x.PeopleCount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("People count must be greater than or equal to 0.");

        RuleFor(x => x.Direction)
            .NotNull()
            .WithMessage("Direction is required.");
    }
}
