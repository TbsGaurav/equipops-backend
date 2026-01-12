using Dapper;
using InterviewService.Api.Helpers.EncryptionHelpers.Handlers;
using InterviewService.Api.Infrastructure.Interface;
using InterviewService.Api.ViewModels.Request.Interview_transcript;
using InterviewService.Api.ViewModels.Request.Webhook;
using InterviewService.Api.ViewModels.Response.Interview_Detail;
using Newtonsoft.Json;
using System.Text.Json;

namespace InterviewService.Api.Infrastructure.Repositories
{
    public class WebHookRepository (EncryptionHelper encryptionHelper,IDbConnectionFactory connectionFactory, ILogger<WebHookRepository> _logger): IWebHookRepository
    {
        public async Task HandleCallEndedAsync(string callId, JsonElement body)
        {
            _logger.LogInformation("HandleCallEndedAsync START");

            string? recordUrl = null;
            DateTimeOffset endTime = DateTimeOffset.UtcNow;

            if (body.TryGetProperty("call", out var callProp))
            {
                if (callProp.TryGetProperty("recording_url", out var recordProp))
                {
                    recordUrl = recordProp.GetString();
                }

                if (callProp.TryGetProperty("end_timestamp", out var endTsProp)
                    && endTsProp.ValueKind == JsonValueKind.Number)
                {
                    endTime = DateTimeOffset.FromUnixTimeMilliseconds(
                        endTsProp.GetInt64()
                    );
                }
            }

            await UpdateCallEndTimeAsync(
                callId,
                endTime,
                recordUrl
            );

            _logger.LogInformation("HandleCallEndedAsync END");            
        }
        //public async Task<string?> GetOpenAiKey(Guid organizationId, Guid interviewId)
        //{
        //    using var conn = connectionFactory.CreateConnection();

        //    var sql = "";
        //    var encrypted = "";
        //    if (organizationId == Guid.Empty)
        //    {
        //        sql = @"
        //          SELECT os.value
        //          FROM master.organization_Setting as os
        //           inner join interviews.interviews as i on i.organization_id = os.organization_id
        //          WHERE i.id = @OrgId 
        //            AND os.key = 'open_ai_api_key'
        //          LIMIT 1;
        //        ";
        //        encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = interviewId });
        //    }
        //    else
        //    {
        //        sql = @"
        //          SELECT value
        //          FROM master.organization_Setting
        //          WHERE organization_id = @OrgId 
        //            AND key = 'open_ai_api_key'
        //          LIMIT 1;
        //        ";
        //        encrypted = await conn.QueryFirstOrDefaultAsync<string>(sql, new { OrgId = organizationId });
        //    }

        //    if (string.IsNullOrEmpty(encrypted))
        //        return null;

        //    var decrypted = encryptionHelper.DecryptFromReact(encrypted);

        //    return decrypted;
        //}
        public async Task HandleCallStartedAsync(string callId)
        {
            try {
                await UpdateCallStartTimeAsync(callId, DateTimeOffset.UtcNow);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<int> UpdateCallStartTimeAsync(string callId, DateTimeOffset startTime)
        {
            using var conn = connectionFactory.CreateConnection();

            var sql = @"
                    UPDATE interviews.candidate_interview_invitation
                    SET start_date_time = @startTime
                    WHERE call_id = @callId;
                ";

            return await conn.ExecuteAsync(sql, new
            {
                callId,
                startTime
            });
        }
        public async Task<int> UpdateCallEndTimeAsync(string callId, DateTimeOffset endTime, string? recordUrl = null)
        {
            using var conn = connectionFactory.CreateConnection();
            var sql = @"
                UPDATE interviews.candidate_interview_invitation
                SET 
                    end_date_time = @endTime,
                    record_url = COALESCE(@recordUrl, record_url)
                WHERE call_id = @callId;
            ";
            return await conn.ExecuteAsync(sql, new
            {
                callId,
                endTime,
                recordUrl
            });
        }
        public async Task<CandidateInterviewInvitationViewModel?> GetCandidateInterviewByCallIdAsync(string callId)
        {
            using var conn = connectionFactory.CreateConnection();

            const string sql = @"
                SELECT 
                    id                  AS ""Id"",
                    organization_id     AS ""OrganizationId"",
                    candidate_id        AS ""CandidateId"",
                    interview_id        AS ""InterviewId""
                FROM interviews.candidate_interview_invitation
                WHERE call_id = @callId
                LIMIT 1;
            ";

            return await conn.QueryFirstOrDefaultAsync<CandidateInterviewInvitationViewModel>(
                sql,
                new { callId }
            );
        }


    }
    public static class JsonElementExtensions
    {
        public static string? GetStringSafe(this JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value) &&
                   value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }

        public static decimal? GetDecimalSafe(this JsonElement element, string property)
        {
            return element.TryGetProperty(property, out var value) &&
                   value.TryGetDecimal(out var result)
                ? result
                : null;
        }
        public static string? GetRecordingUrl(this JsonElement data)
        {
            if (data.TryGetProperty("recording", out var recordingObj))
            {
                var url = recordingObj.GetStringSafe("url");
                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }
            if (data.TryGetProperty("call", out var callObj))
            {
                var url = callObj.GetStringSafe("recording_url");
                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }

            return null;
        }
    }

}
