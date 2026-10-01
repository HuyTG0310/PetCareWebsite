using FluentValidation;

namespace PetCareBooking.Application.Features.Services.Commands.CreateService
{
    public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        public CreateServiceCommandValidator()
        {
            RuleFor(x => x.ServiceName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Tên dịch vụ không được để trống.")
                .MaximumLength(150).WithMessage("Tên dịch vụ không được vượt quá 150 ký tự.");

            RuleFor(x => x.ServiceType)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Loại dịch vụ không được để trống.")
                .Must(x => x == "GROOMING" || x == "BOARDING").WithMessage("Loại dịch vụ chỉ được là GROOMING hoặc BOARDING.");

            RuleFor(x => x.PricingUnit)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Đơn vị giá không được để trống.")
                .Must(x => x == "SESSION" || x == "HOUR" || x == "DAY" || x == "NIGHT")
                .WithMessage("Đơn vị giá không hợp lệ (SESSION, HOUR, DAY, NIGHT).");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Giá dịch vụ không được nhỏ hơn 0.");

            RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes.HasValue)
            .WithMessage("Thời lượng dịch vụ (phút) phải lớn hơn 0.");
        }
    }
}
