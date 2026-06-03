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
        var parameters = new Dictionary<string, string?>
        {
            ["sortBy"] = request.SortBy,
            ["descending"] = request.Descending.ToString(),
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
            parameters["search"] = request.Search.Trim();

        if (request.RoleId.HasValue)
            parameters["roleId"] = request.RoleId.Value.ToString();

        if (request.IsActive.HasValue)
            parameters["isActive"] = request.IsActive.Value.ToString();

        if (request.CreatedFrom.HasValue)
            parameters["createdFrom"] = request.CreatedFrom.Value.ToString("yyyy-MM-dd");

        if (request.CreatedTo.HasValue)
            parameters["createdTo"] = request.CreatedTo.Value.ToString("yyyy-MM-dd");

        var url = QueryHelpers.AddQueryString("api/users", parameters);

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

        var responseData = await response.Content.ReadFromJsonAsync<CreateUserResponse>(cancellationToken);

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
    
    public async Task<UserStatsDto?> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<UserStatsDto>(
            "api/users/stats",
            cancellationToken);
    }
    
    public async Task ChangeStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsJsonAsync(
            $"api/users/{userId}/status",
            new ChangeUserStatusRequest(isActive),
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}