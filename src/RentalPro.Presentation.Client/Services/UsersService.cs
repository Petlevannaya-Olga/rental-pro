using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection.PortableExecutable;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using RentalPro.Contracts.Users;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class UsersService(HttpClient httpClient)
{
    public async Task<PagedResult<UserDto>?> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = QueryHelpers.AddQueryString(
            "api/users",
            new Dictionary<string, string?>
            {
                ["search"] = request.Search,
                ["roleId"] = request.RoleId?.ToString(),
                ["isActive"] = request.IsActive?.ToString(),
                ["createdFrom"] = request.CreatedFrom?.ToString("yyyy-MM-dd"),
                ["createdTo"] = request.CreatedTo?.ToString("yyyy-MM-dd"),
                ["sortBy"] = request.SortBy,
                ["descending"] = request.Descending.ToString(),
                ["page"] = request.Page.ToString(),
                ["pageSize"] = request.PageSize.ToString()
            });
        
        Console.WriteLine(url);

        return await httpClient.GetFromJsonAsync<PagedResult<UserDto>>(
            url,
            cancellationToken);
    }

    public async Task<Result<UserOnlyNameDto, Errors>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/users",
            request,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            return new Errors(
            [
                new Error(
                    "user.create.failed",
                    string.IsNullOrWhiteSpace(content)
                        ? $"Failed to create user. Status code: {(int)response.StatusCode}"
                        : content,
                    ErrorType.FAILURE)
            ]);
        }

        var responseData = await response.Content.ReadFromJsonAsync<CreateUserResponse>(
            cancellationToken);

        if (responseData is null)
        {
            return new Errors(
            [
                new Error(
                    "user.create.empty.response",
                    "Server returned empty response",
                    ErrorType.FAILURE)
            ]);
        }

        return new UserOnlyNameDto(request.LastName, request.FirstName, request.MiddleName);
    }
}