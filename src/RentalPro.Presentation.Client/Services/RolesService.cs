using System.Net.Http.Json;
using RentalPro.Contracts.Roles;

namespace RentalPro.Presentation.Client.Services;

public sealed class RolesService(HttpClient httpClient)
{
    public async Task<List<RoleDto>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<RoleDto>>(
            "api/roles",
            cancellationToken) ?? [];
    }
}