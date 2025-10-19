using Microsoft.Extensions.DependencyInjection;

namespace BookingPlatform.BuildingBlocks.Authentication
{
    // Marker to ensure namespace is compiled
    public static class AuthenticationAssemblyMarker
    {
        public static IServiceCollection AddAuthenticationBuildingBlock(this IServiceCollection services)
        {
            return services;
        }
    }
}
