using System.Net.Http.Json;
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

        return await httpClient.GetFromJsonAsync<PagedResult<UserDto>>(
            url,
            cancellationToken);
    }
    
    public async Task<Result<UserDto, Errors>> CreateUserAsync(
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

        var user = await response.Content.ReadFromJsonAsync<UserDto>(
            cancellationToken);

        if (user is null)
        {
            return new Errors(
            [
                new Error(
                    "user.create.empty.response",
                    "Server returned empty user",
                    ErrorType.FAILURE)
            ]);
        }

        return user;
    }
}