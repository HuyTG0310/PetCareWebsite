using FluentValidation;

namespace PetCareBooking.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("Mã OTP không được để trống.")
                .Length(6).WithMessage("Mã OTP phải có 6 chữ số.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu mới tối thiểu phải 6 ký tự.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Vui lòng xác nhận lại mật khẩu mới.")
                .Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không trùng khớp.");
        }
    }
}