using InternTrack.Api.Controllers;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace InternTrack.Tests;

public class ApiConventionTests
{
    [Fact]
    public async Task ApiDescriptions_ShouldPreserveRoutesAndDescribeEveryControllerAction()
    {
        // Exercise MVC's actual convention matching, rather than inspecting attributes alone.
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(AuthController).Assembly.FullName,
            ContentRootPath = AppContext.BaseDirectory,
            EnvironmentName = "Testing"
        });
        builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
        await using var app = builder.Build();
        app.MapControllers();
        var descriptions = app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(group => group.Items).ToDictionary(description =>
            {
                var action = (ControllerActionDescriptor)description.ActionDescriptor;
                return $"{action.ControllerName}.{action.ActionName}";
            });

        var expected = new (string Action, string Method, string Route, int[] StatusCodes)[]
        {
            ("Auth.Register", "POST", "api/auth/register", [201, 400, 409, 429]),
            ("Auth.Login", "POST", "api/auth/login", [200, 400, 401, 429]),
            ("Auth.Me", "GET", "api/auth/me", [200, 401]),
            ("Auth.UpdateAvatar", "PUT", "api/auth/avatar", [200, 400, 401, 404]),
            ("Auth.UpdateProfile", "PUT", "api/auth/profile", [200, 400, 401, 404, 409]),
            ("Auth.ChangePassword", "PUT", "api/auth/change-password", [200, 400, 401, 404]),
            ("Auth.Refresh", "POST", "api/auth/refresh", [200, 400, 401, 429]),
            ("Auth.Logout", "POST", "api/auth/logout", [200]),
            ("Dashboard.GetStats", "GET", "api/dashboard", [200, 401, 404]),
            ("Department.GetActiveDepartments", "GET", "api/departments", [200]),
            ("Department.GetAllIncludingInactive", "GET", "api/departments/get-all", [200, 401, 403]),
            ("Department.GetDepartmentById", "GET", "api/departments/get-by-id/{id}", [200, 401, 404]),
            ("Department.CreateDepartment", "POST", "api/departments", [201, 400, 401, 403, 409]),
            ("Department.Update", "PUT", "api/departments/update-by-id/{id}", [200, 400, 401, 403, 404, 409]),
            ("Department.DeactivateDepartment", "DELETE", "api/departments/{id}", [204, 401, 403, 404, 409]),
            ("Department.ReactivateDepartment", "PATCH", "api/departments/{id}/restore", [200, 401, 403, 404, 409]),
            ("Intern.GetAll", "GET", "api/interns", [200, 401, 404]),
            ("Intern.GetAllIncludingInactive", "GET", "api/interns/all", [200, 401, 403]),
            ("Intern.GetById", "GET", "api/interns/{id}", [200, 401, 403, 404]),
            ("Intern.CreateInternWithAccount", "POST", "api/interns", [201, 400, 401, 403, 409]),
            ("Intern.Update", "PUT", "api/interns/{id}", [200, 400, 401, 403, 404, 409]),
            ("Intern.DeactivateIntern", "DELETE", "api/interns/{id}", [204, 401, 403, 404]),
            ("Intern.ReactivateIntern", "PATCH", "api/interns/{id}/restore", [200, 401, 403, 404, 409]),
            ("Task.GetAll", "GET", "api/tasks", [200, 401, 404]),
            ("Task.GetAllIncludingInactive", "GET", "api/tasks/all", [200, 401, 403]),
            ("Task.GetById", "GET", "api/tasks/{id}", [200, 401, 403, 404]),
            ("Task.CreateTask", "POST", "api/tasks", [201, 400, 401, 403, 404]),
            ("Task.UpdateTask", "PUT", "api/tasks/{id}", [200, 400, 401, 403, 404]),
            ("Task.DeactivateTask", "DELETE", "api/tasks/{id}", [204, 401, 403, 404]),
            ("Task.ReactivateTask", "PATCH", "api/tasks/{id}/restore", [200, 401, 403, 404, 409])
        };

        Assert.Equal(expected.Length, descriptions.Count);
        foreach (var endpoint in expected)
        {
            Assert.True(descriptions.TryGetValue(endpoint.Action, out var description), endpoint.Action);
            Assert.Equal(endpoint.Method, description.HttpMethod);
            Assert.Equal(endpoint.Route, description.RelativePath);
            var action = (ControllerActionDescriptor)description.ActionDescriptor;
            var anonymous = action.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute));
            var expectedAnonymous = endpoint.Action is "Auth.Register" or "Auth.Login" or "Auth.Refresh"
                or "Auth.Logout" or "Department.GetActiveDepartments";
            Assert.Equal(expectedAnonymous, anonymous);
            var authorization = action.ControllerTypeInfo.GetCustomAttributes<AuthorizeAttribute>()
                .Concat(action.MethodInfo.GetCustomAttributes<AuthorizeAttribute>()).ToArray();
            Assert.Equal(!expectedAnonymous, authorization.Length > 0);
            var expectedRoles = endpoint.Action switch
            {
                "Department.CreateDepartment" or "Department.Update" or "Intern.CreateInternWithAccount" or "Intern.Update" => "Admin,HR",
                "Department.DeactivateDepartment" or "Intern.DeactivateIntern" => "Admin",
                _ when action.ActionName is "GetAllIncludingInactive" or "ReactivateDepartment" or "ReactivateIntern" or "ReactivateTask" => "Admin",
                _ => ""
            };
            Assert.Equal(expectedRoles, string.Join("|", authorization.Select(attribute => attribute.Roles).Where(role => !string.IsNullOrEmpty(role))));
            var statusCodes = description.SupportedResponseTypes.Select(response => response.StatusCode).Order().ToArray();
            Assert.True(endpoint.StatusCodes.SequenceEqual(statusCodes),
                $"{endpoint.Action}: expected [{string.Join(", ", endpoint.StatusCodes)}], got [{string.Join(", ", statusCodes)}].");
        }
    }
}
