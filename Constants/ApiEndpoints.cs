using System;

namespace Toyo_cable_UI.Constants
{
    public static class ApiEndpoints
    {
        public const string BaseUrl = "http://toyocable.runasp.net/api/";
        public const string Login = "Auth/Login";
        public const string Products = "Product";
        public const string Categories = "Category";
        public static string UpdateCategory(Guid id) => $"Category/{id}";
        public static string ProductWithId(Guid id) => $"Product/{id}";
    }
}
