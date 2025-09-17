namespace ApiIntegrationTests.Definitions;

public static class ApiV1Definition
{
    private const string ApiBase = "/api";

    // ==================== AUTHENTICATION & AUTHORIZATION ====================
    public static class Auth
    {
        public static string Login = $"{ApiBase}/auth/login";
        public static string Register = $"{ApiBase}/auth/register";
        public static string Refresh = $"{ApiBase}/auth/refresh";
        public static string Logout = $"{ApiBase}/auth/logout";
        public static string ChangePassword = $"{ApiBase}/auth/change-password";
    }

    // ==================== USERS MANAGEMENT ====================
    public static class Users
    {
        // Current user endpoints
        public static string GetMe = $"{ApiBase}/users/me";
        public static string UpdateMe = $"{ApiBase}/users/me";
        public static string DeleteMe = $"{ApiBase}/users/me";

        // Admin only - User management
        public static string GetAll = $"{ApiBase}/users";
        public static string Create = $"{ApiBase}/users";
        
        /// <summary>
        /// Get user by ID. Use: string.Format(Users.GetById, userId)
        /// </summary>
        public static string GetById = $"{ApiBase}/users/{{0}}";
        
        /// <summary>
        /// Update user by ID. Use: string.Format(Users.UpdateById, userId)
        /// </summary>
        public static string UpdateById = $"{ApiBase}/users/{{0}}";
        
        /// <summary>
        /// Delete user by ID. Use: string.Format(Users.DeleteById, userId)
        /// </summary>
        public static string DeleteById = $"{ApiBase}/users/{{0}}";

        // Reminder settings
        public static string GetReminderSettings = $"{ApiBase}/users/me/reminder-settings";
        public static string UpdateReminderSettings = $"{ApiBase}/users/me/reminder-settings";
    }

    // ==================== ORGANIZATIONS (FLEET/SAAS) ====================
    public static class Organizations
    {
        // Current organization
        public static string GetMe = $"{ApiBase}/organizations/me";
        public static string UpdateMe = $"{ApiBase}/organizations/me";

        /// <summary>
        /// Get organization users. Use: string.Format(Organizations.GetUsers, orgId)
        /// </summary>
        public static string GetUsers = $"{ApiBase}/organizations/{{0}}/users";
        
        /// <summary>
        /// Invite user to organization. Use: string.Format(Organizations.InviteUser, orgId)
        /// </summary>
        public static string InviteUser = $"{ApiBase}/organizations/{{0}}/users";
        
        /// <summary>
        /// Remove user from organization. Use: string.Format(Organizations.RemoveUser, orgId, userId)
        /// </summary>
        public static string RemoveUser = $"{ApiBase}/organizations/{{0}}/users/{{1}}";

        /// <summary>
        /// Get organization vehicles. Use: string.Format(Organizations.GetVehicles, orgId)
        /// </summary>
        public static string GetVehicles = $"{ApiBase}/organizations/{{0}}/vehicles";
        
        /// <summary>
        /// Create vehicle for organization. Use: string.Format(Organizations.CreateVehicle, orgId)
        /// </summary>
        public static string CreateVehicle = $"{ApiBase}/organizations/{{0}}/vehicles";
        
        /// <summary>
        /// Get organization vehicle. Use: string.Format(Organizations.GetVehicleById, orgId, vehicleId)
        /// </summary>
        public static string GetVehicleById = $"{ApiBase}/organizations/{{0}}/vehicles/{{1}}";
        
        /// <summary>
        /// Update organization vehicle. Use: string.Format(Organizations.UpdateVehicle, orgId, vehicleId)
        /// </summary>
        public static string UpdateVehicle = $"{ApiBase}/organizations/{{0}}/vehicles/{{1}}";
        
        /// <summary>
        /// Delete organization vehicle. Use: string.Format(Organizations.DeleteVehicle, orgId, vehicleId)
        /// </summary>
        public static string DeleteVehicle = $"{ApiBase}/organizations/{{0}}/vehicles/{{1}}";

        // Organization analytics
        /// <summary>
        /// Fleet overview analytics. Use: string.Format(Organizations.FleetOverview, orgId)
        /// </summary>
        public static string FleetOverview = $"{ApiBase}/organizations/{{0}}/analytics/fleet-overview";
        
        /// <summary>
        /// Cost analysis. Use: string.Format(Organizations.CostAnalysis, orgId)
        /// </summary>
        public static string CostAnalysis = $"{ApiBase}/organizations/{{0}}/analytics/cost-analysis";
        
        /// <summary>
        /// Efficiency comparison. Use: string.Format(Organizations.EfficiencyComparison, orgId)
        /// </summary>
        public static string EfficiencyComparison = $"{ApiBase}/organizations/{{0}}/analytics/efficiency-comparison";

        // Import/Export
        /// <summary>
        /// Export organization data. Use: string.Format(Organizations.Export, orgId)
        /// </summary>
        public static string Export = $"{ApiBase}/organizations/{{0}}/export";
        
        /// <summary>
        /// Import organization data. Use: string.Format(Organizations.Import, orgId)
        /// </summary>
        public static string Import = $"{ApiBase}/organizations/{{0}}/import";
    }

    // ==================== VEHICLES MANAGEMENT ====================
    public static class Vehicles
    {
        // Context-aware endpoints (single codebase for all deployment modes)
        public static string GetAll = $"{ApiBase}/vehicles";
        public static string Create = $"{ApiBase}/vehicles";
        
        /// <summary>
        /// Get vehicle by ID. Use: string.Format(Vehicles.GetById, vehicleId)
        /// </summary>
        public static string GetById = $"{ApiBase}/vehicles/{{0}}";
        
        /// <summary>
        /// Update vehicle. Use: string.Format(Vehicles.UpdateById, vehicleId)
        /// </summary>
        public static string UpdateById = $"{ApiBase}/vehicles/{{0}}";
        
        /// <summary>
        /// Delete vehicle. Use: string.Format(Vehicles.DeleteById, vehicleId)
        /// </summary>
        public static string DeleteById = $"{ApiBase}/vehicles/{{0}}";

        // Vehicle permissions (Future: Fleet/SAAS only)
        /// <summary>
        /// Get vehicle permissions. Use: string.Format(Vehicles.GetPermissions, vehicleId)
        /// </summary>
        public static string GetPermissions = $"{ApiBase}/vehicles/{{0}}/permissions";
        
        /// <summary>
        /// Grant vehicle access. Use: string.Format(Vehicles.GrantPermission, vehicleId)
        /// </summary>
        public static string GrantPermission = $"{ApiBase}/vehicles/{{0}}/permissions";
        
        /// <summary>
        /// Update user permissions. Use: string.Format(Vehicles.UpdatePermission, vehicleId, userId)
        /// </summary>
        public static string UpdatePermission = $"{ApiBase}/vehicles/{{0}}/permissions/{{1}}";
        
        /// <summary>
        /// Remove vehicle access. Use: string.Format(Vehicles.RemovePermission, vehicleId, userId)
        /// </summary>
        public static string RemovePermission = $"{ApiBase}/vehicles/{{0}}/permissions/{{1}}";

        // Vehicle analytics
        /// <summary>
        /// Fuel consumption analytics. Use: string.Format(Vehicles.FuelConsumption, vehicleId)
        /// </summary>
        public static string FuelConsumption = $"{ApiBase}/vehicles/{{0}}/analytics/fuel-consumption";
        
        /// <summary>
        /// Cost analytics. Use: string.Format(Vehicles.CostAnalytics, vehicleId)
        /// </summary>
        public static string CostAnalytics = $"{ApiBase}/vehicles/{{0}}/analytics/costs";
        
        /// <summary>
        /// Efficiency analytics. Use: string.Format(Vehicles.Efficiency, vehicleId)
        /// </summary>
        public static string Efficiency = $"{ApiBase}/vehicles/{{0}}/analytics/efficiency";

        // Import/Export
        /// <summary>
        /// Export vehicle data. Use: string.Format(Vehicles.Export, vehicleId)
        /// </summary>
        public static string Export = $"{ApiBase}/vehicles/{{0}}/export";
        
        /// <summary>
        /// Export energy entries. Use: string.Format(Vehicles.ExportEnergyEntries, vehicleId)
        /// </summary>
        public static string ExportEnergyEntries = $"{ApiBase}/vehicles/{{0}}/export/energy-entries";
        
        /// <summary>
        /// Export service records. Use: string.Format(Vehicles.ExportServices, vehicleId)
        /// </summary>
        public static string ExportServices = $"{ApiBase}/vehicles/{{0}}/export/services";

        // Import
        public static string Import = $"{ApiBase}/vehicles/import";
        
        /// <summary>
        /// Import energy entries. Use: string.Format(Vehicles.ImportEnergyEntries, vehicleId)
        /// </summary>
        public static string ImportEnergyEntries = $"{ApiBase}/vehicles/{{0}}/import/energy-entries";
        
        /// <summary>
        /// Import service records. Use: string.Format(Vehicles.ImportServices, vehicleId)
        /// </summary>
        public static string ImportServices = $"{ApiBase}/vehicles/{{0}}/import/services";
    }

    // ==================== ENERGY TYPES ====================
    public static class EnergyTypes
    {
        /// <summary>
        /// Get vehicle energy types. Use: string.Format(EnergyTypes.GetAll, vehicleId)
        /// </summary>
        public static string GetAll = $"{ApiBase}/vehicles/{{0}}/energy-types";
        
        /// <summary>
        /// Add energy type to vehicle. Use: string.Format(EnergyTypes.Add, vehicleId)
        /// </summary>
        public static string Add = $"{ApiBase}/vehicles/{{0}}/energy-types";
        
        /// <summary>
        /// Remove energy type from vehicle. Use: string.Format(EnergyTypes.Remove, vehicleId, energyTypeId)
        /// </summary>
        public static string Remove = $"{ApiBase}/vehicles/{{0}}/energy-types/{{1}}";
    }

    // ==================== ENERGY ENTRIES (FUEL/CHARGE HISTORY) ====================
    public static class EnergyEntries
    {
        /// <summary>
        /// Get vehicle energy entries. Use: string.Format(EnergyEntries.GetAll, vehicleId)
        /// </summary>
        public static string GetAll = $"{ApiBase}/vehicles/{{0}}/energy-entries";
        
        /// <summary>
        /// Create energy entry. Use: string.Format(EnergyEntries.Create, vehicleId)
        /// </summary>
        public static string Create = $"{ApiBase}/vehicles/{{0}}/energy-entries";
        
        /// <summary>
        /// Get specific energy entry. Use: string.Format(EnergyEntries.GetById, vehicleId, entryId)
        /// </summary>
        public static string GetById = $"{ApiBase}/vehicles/{{0}}/energy-entries/{{1}}";
        
        /// <summary>
        /// Update energy entry. Use: string.Format(EnergyEntries.UpdateById, vehicleId, entryId)
        /// </summary>
        public static string UpdateById = $"{ApiBase}/vehicles/{{0}}/energy-entries/{{1}}";
        
        /// <summary>
        /// Delete energy entry. Use: string.Format(EnergyEntries.DeleteById, vehicleId, entryId)
        /// </summary>
        public static string DeleteById = $"{ApiBase}/vehicles/{{0}}/energy-entries/{{1}}";

        // Bulk operations
        /// <summary>
        /// Bulk create energy entries. Use: string.Format(EnergyEntries.BulkCreate, vehicleId)
        /// </summary>
        public static string BulkCreate = $"{ApiBase}/vehicles/{{0}}/energy-entries/bulk";
        
        /// <summary>
        /// Bulk update energy entries. Use: string.Format(EnergyEntries.BulkUpdate, vehicleId)
        /// </summary>
        public static string BulkUpdate = $"{ApiBase}/vehicles/{{0}}/energy-entries/bulk";
        
        /// <summary>
        /// Bulk delete energy entries. Use: string.Format(EnergyEntries.BulkDelete, vehicleId)
        /// </summary>
        public static string BulkDelete = $"{ApiBase}/vehicles/{{0}}/energy-entries/bulk";

        // Analytics
        /// <summary>
        /// Energy consumption statistics. Use: string.Format(EnergyEntries.GetStats, vehicleId)
        /// </summary>
        public static string GetStats = $"{ApiBase}/vehicles/{{0}}/energy-entries/stats";
        
        /// <summary>
        /// Export energy entries. Use: string.Format(EnergyEntries.Export, vehicleId)
        /// </summary>
        public static string Export = $"{ApiBase}/vehicles/{{0}}/energy-entries/export";
    }

    // ==================== SERVICE RECORDS ====================
    public static class Services
    {
        /// <summary>
        /// Get service records. Use: string.Format(Services.GetAll, vehicleId)
        /// </summary>
        public static string GetAll = $"{ApiBase}/vehicles/{{0}}/services";
        
        /// <summary>
        /// Create service record. Use: string.Format(Services.Create, vehicleId)
        /// </summary>
        public static string Create = $"{ApiBase}/vehicles/{{0}}/services";
        
        /// <summary>
        /// Get specific service. Use: string.Format(Services.GetById, vehicleId, serviceId)
        /// </summary>
        public static string GetById = $"{ApiBase}/vehicles/{{0}}/services/{{1}}";
        
        /// <summary>
        /// Update service record. Use: string.Format(Services.UpdateById, vehicleId, serviceId)
        /// </summary>
        public static string UpdateById = $"{ApiBase}/vehicles/{{0}}/services/{{1}}";
        
        /// <summary>
        /// Delete service record. Use: string.Format(Services.DeleteById, vehicleId, serviceId)
        /// </summary>
        public static string DeleteById = $"{ApiBase}/vehicles/{{0}}/services/{{1}}";

        // Scheduled services
        /// <summary>
        /// Get scheduled services. Use: string.Format(Services.GetScheduled, vehicleId)
        /// </summary>
        public static string GetScheduled = $"{ApiBase}/vehicles/{{0}}/services/scheduled";
        
        /// <summary>
        /// Schedule service. Use: string.Format(Services.Schedule, vehicleId)
        /// </summary>
        public static string Schedule = $"{ApiBase}/vehicles/{{0}}/services/scheduled";
        
        /// <summary>
        /// Update scheduled service. Use: string.Format(Services.UpdateScheduled, vehicleId, serviceId)
        /// </summary>
        public static string UpdateScheduled = $"{ApiBase}/vehicles/{{0}}/services/scheduled/{{1}}";
        
        /// <summary>
        /// Cancel scheduled service. Use: string.Format(Services.CancelScheduled, vehicleId, serviceId)
        /// </summary>
        public static string CancelScheduled = $"{ApiBase}/vehicles/{{0}}/services/scheduled/{{1}}";

        // Service templates
        /// <summary>
        /// Get service templates. Use: string.Format(Services.GetTemplates, vehicleId)
        /// </summary>
        public static string GetTemplates = $"{ApiBase}/vehicles/{{0}}/service-templates";
        
        /// <summary>
        /// Create service template. Use: string.Format(Services.CreateTemplate, vehicleId)
        /// </summary>
        public static string CreateTemplate = $"{ApiBase}/vehicles/{{0}}/service-templates";
        
        /// <summary>
        /// Update service template. Use: string.Format(Services.UpdateTemplate, vehicleId, templateId)
        /// </summary>
        public static string UpdateTemplate = $"{ApiBase}/vehicles/{{0}}/service-templates/{{1}}";
        
        /// <summary>
        /// Delete service template. Use: string.Format(Services.DeleteTemplate, vehicleId, templateId)
        /// </summary>
        public static string DeleteTemplate = $"{ApiBase}/vehicles/{{0}}/service-templates/{{1}}";
    }

    // ==================== INSURANCE MANAGEMENT ====================
    public static class Insurance
    {
        /// <summary>
        /// Get current insurance. Use: string.Format(Insurance.GetCurrent, vehicleId)
        /// </summary>
        public static string GetCurrent = $"{ApiBase}/vehicles/{{0}}/insurance";
        
        /// <summary>
        /// Add insurance. Use: string.Format(Insurance.Add, vehicleId)
        /// </summary>
        public static string Add = $"{ApiBase}/vehicles/{{0}}/insurance";
        
        /// <summary>
        /// Update insurance. Use: string.Format(Insurance.Update, vehicleId, insuranceId)
        /// </summary>
        public static string Update = $"{ApiBase}/vehicles/{{0}}/insurance/{{1}}";
        
        /// <summary>
        /// Remove insurance. Use: string.Format(Insurance.Remove, vehicleId, insuranceId)
        /// </summary>
        public static string Remove = $"{ApiBase}/vehicles/{{0}}/insurance/{{1}}";

        // Insurance history
        /// <summary>
        /// Get insurance history. Use: string.Format(Insurance.GetHistory, vehicleId)
        /// </summary>
        public static string GetHistory = $"{ApiBase}/vehicles/{{0}}/insurance/history";
    }

    // ==================== REMINDERS & NOTIFICATIONS ====================
    public static class Reminders
    {
        // General reminders
        public static string GetAll = $"{ApiBase}/reminders";
        public static string Create = $"{ApiBase}/reminders";
        
        /// <summary>
        /// Get reminder details. Use: string.Format(Reminders.GetById, reminderId)
        /// </summary>
        public static string GetById = $"{ApiBase}/reminders/{{0}}";
        
        /// <summary>
        /// Update reminder. Use: string.Format(Reminders.UpdateById, reminderId)
        /// </summary>
        public static string UpdateById = $"{ApiBase}/reminders/{{0}}";
        
        /// <summary>
        /// Delete reminder. Use: string.Format(Reminders.DeleteById, reminderId)
        /// </summary>
        public static string DeleteById = $"{ApiBase}/reminders/{{0}}";

        // Vehicle-specific reminders
        /// <summary>
        /// Get vehicle reminders. Use: string.Format(Reminders.GetVehicleReminders, vehicleId)
        /// </summary>
        public static string GetVehicleReminders = $"{ApiBase}/vehicles/{{0}}/reminders";
        
        /// <summary>
        /// Create vehicle reminder. Use: string.Format(Reminders.CreateVehicleReminder, vehicleId)
        /// </summary>
        public static string CreateVehicleReminder = $"{ApiBase}/vehicles/{{0}}/reminders";

        // System-generated reminders (read-only)
        public static string InsuranceExpiring = $"{ApiBase}/reminders/insurance-expiring";
        public static string ServicesDue = $"{ApiBase}/reminders/services-due";
        public static string InspectionsDue = $"{ApiBase}/reminders/inspections-due";
    }
}

// ==================== HELPER EXTENSIONS ====================
/// <summary>
/// Extension methods to make URL formatting cleaner
/// </summary>
public static class ApiEndpointExtensions
{
    /// <summary>
    /// Format URL with single parameter
    /// Usage: ApiV1Definition.Users.GetById.WithId(userId)
    /// </summary>
    public static string WithId(this string urlTemplate, object id)
        => string.Format(urlTemplate, id);

    /// <summary>
    /// Format URL with two parameters  
    /// Usage: ApiV1Definition.Organizations.RemoveUser.WithIds(orgId, userId)
    /// </summary>
    public static string WithIds(this string urlTemplate, object id1, object id2)
        => string.Format(urlTemplate, id1, id2);

    /// <summary>
    /// Format URL with multiple parameters
    /// Usage: urlTemplate.WithParams(param1, param2, param3)
    /// </summary>
    public static string WithParams(this string urlTemplate, params object[] parameters)
        => string.Format(urlTemplate, parameters);
}

// ==================== USAGE EXAMPLES ====================
/*
// Example usage in tests:

public class VehicleTests : BaseIntegrationTest
{
    [Fact]
    public async Task GetVehicle_ValidId_ReturnsVehicle()
    {
        var vehicleId = Guid.NewGuid();
        var response = await Client.GetAsync(
            ApiV1Definition.Vehicles.GetById.WithId(vehicleId));
        
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AddEnergyEntry_ValidData_Success()
    {
        var vehicleId = Guid.NewGuid();
        var entryData = new { Amount = 50.5, Date = DateTime.UtcNow, Cost = 200 };
        
        var response = await Client.PostAsJsonAsync(
            ApiV1Definition.EnergyEntries.Create.WithId(vehicleId), 
            entryData);
            
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task RemoveUserFromOrganization_ValidIds_Success()
    {
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var response = await Client.DeleteAsync(
            ApiV1Definition.Organizations.RemoveUser.WithIds(orgId, userId));
            
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetEnergyStats_ValidVehicle_ReturnsStats()
    {
        var vehicleId = Guid.NewGuid();
        
        var response = await Client.GetAsync(
            ApiV1Definition.EnergyEntries.GetStats.WithId(vehicleId));
            
        var stats = await response.Content.ReadFromJsonAsync<EnergyStatsDto>();
        Assert.NotNull(stats);
    }
}
*/