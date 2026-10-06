using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Application.Features.Auth.Commands.ForgotPassword;
using PetCareBooking.Application.Features.Auth.Commands.Login;
using PetCareBooking.Application.Features.Auth.Commands.Logout;
using PetCareBooking.Application.Features.Auth.Commands.Register;
using PetCareBooking.Application.Features.Auth.Commands.ResendOtp;
using PetCareBooking.Application.Features.Auth.Commands.ResetPassword;
using PetCareBooking.Application.Features.Auth.Commands.VerifyOtp;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// 1. Đăng ký tài khoản mới (Gửi mã OTP qua email)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 2. Xác thực mã OTP
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 3. Gửi lại mã OTP
        /// </summary>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 4. Đăng nhập hệ thống (Trả về mã JWT Token)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 5. Đăng xuất hệ thống (Yêu cầu gửi kèm JWT Token)
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var response = await _mediator.Send(new LogoutCommand());
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 6. Yêu cầu đặt lại mật khẩu (Gửi OTP qua email)
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// 7. Đặt lại mật khẩu mới với mã OTP
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var response = await _mediator.Send(command);
            return StatusCode(response.StatusCode, response);
        }
    }
}