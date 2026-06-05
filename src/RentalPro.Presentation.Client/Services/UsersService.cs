using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using RentalPro.Contracts.Users;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class UsersService(HttpClient httpClient, IJSRuntime jsRuntime)
{
    public async Task<Result<PagedResult<UserDto>, Errors>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        try
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

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить пользователей",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "users.load.failed",
                        message)
                    .ToErrors();
            }

            var users = await response.Content
                .ReadFromJsonAsync<PagedResult<UserDto>>(cancellationToken);

            if (users is null)
            {
                return CommonErrors.EmptyResponse(
                        "users.empty.response",
                        "Сервер вернул пустой список пользователей")
                    .ToErrors();
            }

            return users;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "users.load.failed",
                "Не удалось загрузить пользователей");
        }
    }

    public async Task<Result<UserOnlyNameDto, Errors>> CreateUserAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/users",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось создать пользователя",
                    cancellationToken);

                return CommonErrors.CreateFailed(
                        "user.create.failed",
                        message)
                    .ToErrors();
            }

            var responseData = await response.Content
                .ReadFromJsonAsync<CreateUserResponse>(cancellationToken);

            if (responseData is null)
            {
                return CommonErrors.EmptyResponse(
                        "user.create.empty.response",
                        "Сервер вернул пустой ответ")
                    .ToErrors();
            }

            return new UserOnlyNameDto(
                request.LastName,
                request.FirstName,
                request.MiddleName);
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "user.create.failed",
                "Не удалось создать пользователя");
        }
    }

    public async Task<Result<UserStatsDto, Errors>> GetStatsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/users/stats",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось загрузить статистику пользователей",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "users.stats.load.failed",
                        message)
                    .ToErrors();
            }

            var stats = await response.Content
                .ReadFromJsonAsync<UserStatsDto>(cancellationToken);

            if (stats is null)
            {
                return CommonErrors.EmptyResponse(
                        "users.stats.empty.response",
                        "Сервер вернул пустую статистику пользователей")
                    .ToErrors();
            }

            return stats;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "users.stats.load.failed",
                "Не удалось загрузить статистику пользователей");
        }
    }

    public async Task<Result<bool, Errors>> ChangeStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PatchAsJsonAsync(
                $"api/users/{userId}/status",
                new ChangeUserStatusRequest(isActive),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось изменить статус пользователя",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "user.status.update.failed",
                        message)
                    .ToErrors();
            }

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "user.status.update.failed",
                "Не удалось изменить статус пользователя");
        }
    }

    public async Task<Result<bool, Errors>> UpdateUserAsync(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(
                $"api/users/{id}",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось обновить пользователя",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "user.update.failed",
                        message)
                    .ToErrors();
            }

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "user.update.failed",
                "Не удалось обновить пользователя");
        }
    }

    public async Task<Result<bool, Errors>> ChangePasswordAsync(
        Guid userId,
        ChangeUserPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PatchAsJsonAsync(
                $"api/users/{userId}/password",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось изменить пароль пользователя",
                    cancellationToken);

                return CommonErrors.UpdateFailed(
                        "user.password.update.failed",
                        message)
                    .ToErrors();
            }

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "user.password.update.failed",
                "Не удалось изменить пароль пользователя");
        }
    }

    public async Task<Result<bool, Errors>> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync(
                $"api/users/{userId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось удалить пользователя",
                    cancellationToken);

                return CommonErrors.DeleteFailed(
                        "user.delete.failed",
                        message)
                    .ToErrors();
            }

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "user.delete.failed",
                "Не удалось удалить пользователя");
        }
    }

    public async Task<Result<bool, Errors>> ExportUsersAsync(
        ExportUsersRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = QueryHelpers.AddQueryString(
                "api/users/export",
                new Dictionary<string, string?>
                {
                    ["search"] = request.Search,
                    ["roleId"] = request.RoleId?.ToString(),
                    ["isActive"] = request.IsActive?.ToString(),
                    ["createdFrom"] = request.CreatedFrom?.ToString("yyyy-MM-dd"),
                    ["createdTo"] = request.CreatedTo?.ToString("yyyy-MM-dd"),
                    ["sortBy"] = request.SortBy,
                    ["descending"] = request.Descending.ToString()
                });

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var message = await ReadErrorMessageAsync(
                    response,
                    "Не удалось экспортировать пользователей",
                    cancellationToken);

                return CommonErrors.LoadFailed(
                        "users.export.failed",
                        message)
                    .ToErrors();
            }

            var bytes = await response.Content
                .ReadAsByteArrayAsync(cancellationToken);

            await jsRuntime.InvokeVoidAsync(
                "downloadFile",
                cancellationToken,
                "users.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                bytes);

            return true;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "users.export.failed",
                "Не удалось экспортировать пользователей");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        var content = await response.Content
            .ReadAsStringAsync(cancellationToken);

        return content.ExtractErrorMessage(defaultMessage);
    }
}