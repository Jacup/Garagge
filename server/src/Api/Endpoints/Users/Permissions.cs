namespace Api.Endpoints.Users;

internal static class Permissions
{
    internal const string UsersAccess = "users:access";

    internal static class Users
    {
        public const string View = "users:view";
        public const string Manage = "users:manage";
    }

    internal static class Vehicles
    {
        public const string View = "vehicles:view";
        public const string Manage = "vehicles:manage";
        public const string AssignUsers = "vehicles:assign-users";
    }

    internal static class EnergyEntries
    {
        public const string View = "energy:view";
        public const string Manage = "energy:manage";
    }

    internal static class Organizations
    {
        public const string Manage = "org:manage";
    }

    internal static class System
    {
        public const string SuperAdmin = "system:superadmin";
    }
}