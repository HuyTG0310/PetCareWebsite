using FluentValidation;

namespace PetCareBooking.Application.Features.Customers.Commands.UpdateCustomer
{
    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {
        public UpdateCustomerCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id khách hàng không được để trống.");

            RuleFor(x => x.FullName)
                .MaximumLength(100).WithMessage("Họ và tên không vượt quá 100 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Số điện thoại không đúng định dạng (Ví dụ: 0912345678).")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Địa chỉ không vượt quá 255 ký tự.")
                .When(x => !string.IsNullOrEmpty(x.Address));
        }
    }
}
