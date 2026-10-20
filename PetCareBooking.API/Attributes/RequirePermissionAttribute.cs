using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PetCareBooking.Application.Common.Models;
using PetCareBooking.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PetCareBooking.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _permissions;

        public RequirePermissionAttribute(params string[] permissions)
        {
            _permissions = permissions ?? Array.Empty<string>();
        }

        public Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var currentUserService = context.HttpContext.RequestServices.GetService<ICurrentUserService>();
            if (currentUserService == null || !currentUserService.IsAuthenticated)
            {
                context.Result = new ObjectResult(new ApiResponse<object>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "User is not authenticated.",
                    Result = null
                })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return Task.CompletedTask;
            }

            // Admin always has full access to all endpoints
            if (currentUserService.IsInRole("Admin"))
            {
                return Task.CompletedTask;
            }

            // Check if the current user has at least one of the required permissions
            if (_permissions.Length > 0 && !_permissions.Any(p => currentUserService.HasPermission(p)))
            {
                var permsString = string.Join(" hoặc ", _permissions);
                context.Result = new ObjectResult(new ApiResponse<object>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = $"Bạn không có quyền thực hiện thao tác này (Yêu cầu quyền: {permsString}).",
                    Result = null
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}

