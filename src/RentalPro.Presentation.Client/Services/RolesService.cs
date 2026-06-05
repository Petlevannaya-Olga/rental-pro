using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using RentalPro.Contracts.Roles;
using RentalPro.Presentation.Client.Extensions;
using RentalPro.Shared;

namespace RentalPro.Presentation.Client.Services;

public sealed class RolesService(HttpClient httpClient)
{
    public async Task<Result<List<RoleDto>, Errors>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                "api/roles",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content
                    .ReadAsStringAsync(cancellationToken);

                var message = content.ExtractErrorMessage(
                    "Не удалось загрузить роли");

                return CommonErrors.LoadFailed(
                        "roles.load.failed",
                        message)
                    .ToErrors();
            }

            var roles = await response.Content
                .ReadFromJsonAsync<List<RoleDto>>(cancellationToken);

            if (roles is null)
            {
                return CommonErrors.EmptyResponse(
                        "roles.empty.response",
                        "Сервер вернул пустой список ролей")
                    .ToErrors();
            }

            return roles;
        }
        catch (Exception ex)
        {
            return ex.ToErrors(
                "roles.load.failed",
                "Не удалось загрузить роли");
        }
    }
}