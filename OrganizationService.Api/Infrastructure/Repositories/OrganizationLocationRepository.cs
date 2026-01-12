using Dapper;

using Npgsql;

using OrganizationService.Api.Helpers;
using OrganizationService.Api.Infrastructure.Interface;
using OrganizationService.Api.ViewModels.Response.OrganizationLocation;

using System.Data;

namespace OrganizationService.Api.Infrastructure.Repositories
{
    public class OrganizationLocationRepository : IOrganizationLocationRepository
    {

        private readonly IDbConnectionFactory _dbFactory;

        public OrganizationLocationRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region Get Coutry List
        public async Task<CountryListResponseViewModel> GetCountriesAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetCountryList}(@ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var countries = conn.Query<Countries>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new CountryListResponseViewModel
            {
                Countries = countries
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get State By Country List
        public async Task<StateByCountryListResponseViewModel> GetStatesByCountryAsync(Guid countryId)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetStatesByCountryList}(@p_country_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_country_id", countryId);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var states = conn.Query<States>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new StateByCountryListResponseViewModel
            {
                States = states
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion

        #region Get City By State List
        public async Task<CityByStateListResponseViewModel> GetCitiesByStateAsync(Guid stateId)
        {
            using var conn = _dbFactory.CreateConnection();
            conn.Open();

            using var tran = conn.BeginTransaction();

            await using var cmd = new NpgsqlCommand(
                $"CALL {StoreProcedure.GetCitiesByStateList}(@p_state_id, @ref)",
                (NpgsqlConnection)conn
            );
            cmd.Transaction = (NpgsqlTransaction)tran;

            // Input
            cmd.Parameters.AddWithValue("p_state_id", stateId);

            var cursorParam = new NpgsqlParameter("ref", NpgsqlTypes.NpgsqlDbType.Refcursor)
            {
                Direction = ParameterDirection.InputOutput,
                Value = "my_cursor"
            };
            cmd.Parameters.Add(cursorParam);

            // Execute procedure
            cmd.ExecuteNonQuery();

            var cities = conn.Query<Cities>("FETCH ALL IN \"my_cursor\"", transaction: tran).ToList();

            tran.Commit();

            // Map output parameters to entity
            var data = new CityByStateListResponseViewModel
            {
                Cities = cities
            };

            // Return in ResponseViewModel wrapper
            return data;
        }
        #endregion
    }
}
