namespace InvenScan.Routes;

public static class Api
{
    public const string Auth = "api/auth";
    public const string Login = "api/auth/login";
    public const string Refresh = "api/auth/refresh";

    public const string Item = "api/item";
    public const string ItemById = "api/item/{id}";

    public const string Location = "api/location";

    public const string Tag = "api/tag";
    public const string TagById = "api/tag/{id}";
    public const string TagRegister = "api/tag/register";

    public const string StockIn = "api/stockin";
    public const string StockInBulkInfo = "api/stockin/bulk-info";

    public const string StockTaking = "api/stock-taking";
    public const string StockTakingActive = "api/stock-taking/active";
    public const string StockTakingTags = "api/stock-taking/tags/{sttId}";
    public const string StockTakingAvailableTags = "api/stock-taking/available-tags/{sttId}";
    public const string StockTakingOperatorSubmit = "api/stock-taking/operator-submit";

    public const string StockPrep = "api/stockprep";
    public const string StockPrepById = "api/stockprep/{id}";
    public const string StockPrepBulk = "api/stockprep/bulk";

    public const string SearchItem = "api/search-item";
    public const string SearchItemByCode = "api/search-item/{code}";

    public const string User = "api/user";

    public const string Ping = "api/ping";
}
