using Dapper;

using Npgsql;

using SettingService.Api.Helpers;
using SettingService.Api.Infrastructure.Interface;
using SettingService.Api.ViewModels.Response.Master_Dropdown;

using System.Data;

namespace SettingService.Api.Infrastructure.Repositories
{
    public class MasterDropdownRepository : IMasterDropdownRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public MasterDropdownRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region Get MasterDropdown List
        public async Task<MasterDropdownListResponseViewModel> GetMasterDropdownsAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetMasterDropdownList}(@ref_role, @ref_organization, @ref_language, @ref_menu_type, @ref_industry_type, @ref_menu_permission)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            var roleCursor = new NpgsqlParameter("ref_role", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "role_cursor"
            };
            var organizationCursor = new NpgsqlParameter("ref_organization", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "organization_cursor"
            };
            var languageCursor = new NpgsqlParameter("ref_language", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "language_cursor"
            };
            var menuTypeCursor = new NpgsqlParameter("ref_menu_type", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "menu_type_cursor"
            };
            var industryTypeCursor = new NpgsqlParameter("ref_industry_type", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "industry_type_cursor"
            };
            var menuPermissionCursor = new NpgsqlParameter("ref_menu_permission", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "menu_permission_cursor"
            };

            cmd.Parameters.Add(roleCursor);
            cmd.Parameters.Add(organizationCursor);
            cmd.Parameters.Add(languageCursor);
            cmd.Parameters.Add(menuTypeCursor);
            cmd.Parameters.Add(industryTypeCursor);
            cmd.Parameters.Add(menuPermissionCursor);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var roles = conn.Query<MasterDropdownItemViewModel>("FETCH ALL IN \"role_cursor\"", transaction: tran).ToList();
            var organizations = conn.Query<MasterDropdownItemViewModel>("FETCH ALL IN \"organization_cursor\"", transaction: tran).ToList();
            var languages = conn.Query<LanguageDropdownItemViewModel>("FETCH ALL IN \"language_cursor\"", transaction: tran).ToList();
            var menuTypes = conn.Query<MasterDropdownItemViewModel>("FETCH ALL IN \"menu_type_cursor\"", transaction: tran).ToList();
            var industryTypes = conn.Query<IndustryTypeDropdownItemViewModel>("FETCH ALL IN \"industry_type_cursor\"", transaction: tran).ToList();
            var menuPermissions = conn.Query<MenuPermissionDropdownItemViewModel>("FETCH ALL IN \"menu_permission_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new MasterDropdownListResponseViewModel
            {
                Roles = roles,
                Organizations = organizations,
                Languages = languages,
                Menu_Types = menuTypes,
                Industry_Types = industryTypes,
                Menu_Permissions = menuPermissions
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion
    }
}
