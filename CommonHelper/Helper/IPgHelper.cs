using CommonHelper.Helpers;

namespace CommonHelper.Helper
{
    public interface IPgHelper
    {
        Task<dynamic> CreateUpdateAsync(string procedureName, Dictionary<string, DbParam> Params);
        Task<dynamic> ListAsync(string procedureName, Dictionary<string, DbParam> Params);
    };
}