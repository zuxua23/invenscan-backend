namespace InvenScan.Utility;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "ADMIN";
        public const string Operator = "OPERATOR";
    }

    public static class TagStatus
    {
        public const string InStock = "IN_STOCK";
        public const string Out = "OUT";
        public const string Unknown = "UNKNOWN";
    }

    public static class StockTakingStatus
    {
        public const string Open = "OPEN";
        public const string Closed = "CLOSED";
    }

    public static class StockTakingAction
    {
        public const string System = "SYSTEM";
        public const string Scan = "SCAN";
        public const string Missing = "MISSING";
    }

    public static class StockInStatus
    {
        public const string Pending = "PENDING";
        public const string Synced = "SYNCED";
    }

    public static class StockPrepStatus
    {
        public const string Open = "OPEN";
        public const string InProgress = "IN_PROGRESS";
        public const string Done = "DONE";
    }

    public static class StockPrepDetailStatus
    {
        public const string Pending = "PENDING";
        public const string Picked = "PICKED";
    }

    public static class ScanType
    {
        public const string Rfid = "RFID";
        public const string Barcode = "BARCODE";
    }

    public static class AuthSchemes
    {
        public const string Cookie = "CookieAuth";
        public const string Jwt = "Bearer";
    }

    public static class RateLimitPolicies
    {
        public const string Auth = "auth";
    }
}
