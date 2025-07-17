using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using EpinelPSLauncher.Utils;

namespace EpinelPSLauncher.Models
{
    public class ChannelInfo
    {
        public string openid { get; set; } = "";
        public string token { get; set; } = "";
        public int account_type { get; set; }
        public string account { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public int account_plat_type { get; set; }
        public string lang_type { get; set; } = "";
        public bool is_login { get; set; }
        public string account_uid { get; set; } = "";
        public string account_token { get; set; } = "";
    }
    public class AuthPkt
    {
        public ChannelInfo channel_info { get; set; } = new();
    }
    public class AuthPkt2
    {
        public string token { get; set; } = "";
        public string openid { get; set; } = "";
        public string account_token { get; set; } = "";
    }
    public class DeviceInfo
    {
        public string guest_id { get; set; } = "";
        public string lang_type { get; set; } = "";
        public string root_info { get; set; } = "";
        public string app_version { get; set; } = "";
        public string screen_dpi { get; set; } = "";
        public int screen_height { get; set; }
        public int screen_width { get; set; }
        public string device_brand { get; set; } = "";
        public string device_model { get; set; } = "";
        public int network_type { get; set; }
        public int ram_total { get; set; }
        public int rom_total { get; set; }
        public string cpu_name { get; set; } = "";
        public string client_region { get; set; } = "";
        public string vm_type { get; set; } = "";
        public string xwid { get; set; } = "";
        public string new_xwid { get; set; } = "";
        public string xwid_flag { get; set; } = "";
        public string cpu_arch { get; set; } = "";
    }

    public class LoginEndpoint1Req
    {
        public ChannelInfo channel_info { get; set; } = new();
        public DeviceInfo device_info { get; set; } = new();
        public string channel_dis { get; set; } = "";
        public string login_extra_info { get; set; } = "";
        public string lang_type { get; set; } = "";
    }
    public class ExtraJson
    {
        public string del_li_account_info { get; set; } = "";
        public GetStatusRsp get_status_rsp { get; set; } = null!;
        public NeedNotifyRsp need_notify_rsp { get; set; } = null!;
    }

    public class GameGradeMap
    {
    }
    public class AdultAgeMap
    {
    }

    public class AdultStatusMap
    {
    }
    public class TokenizedTimerExpireMap
    {

    }
    public class GetStatusRsp
    {
        public int adult_age { get; set; }
        public AdultAgeMap adult_age_map { get; set; } = null!;
        public int adult_check_status { get; set; }
        public string adult_check_status_expiration { get; set; } = "";
        public AdultStatusMap adult_status_map { get; set; } = null!;
        public int age_max { get; set; }
        public int age_min { get; set; }
        public List<object> age_range { get; set; } = [];
        public int certificate_type { get; set; }
        public string email { get; set; } = "";
        public int email_encrypted { get; set; }
        public int eu_user_agree_status { get; set; }
        public int game_grade { get; set; }
        public GameGradeMap game_grade_map { get; set; } = null!;
        public bool is_dma { get; set; }
        public bool is_eea { get; set; }
        public bool is_need_li_cert { get; set; }
        public int lbs_control_status { get; set; }
        public string msg { get; set; } = "";
        public int need_lbs_control { get; set; }
        public int need_lbs_control_ingame { get; set; }
        public int need_lbs_control_parent_cert { get; set; }
        public int need_parent_control { get; set; }
        public int need_realname_auth { get; set; }
        public int need_voice_control { get; set; }
        public int need_voice_control_ingame { get; set; }
        public int need_voice_control_parent_cert { get; set; }
        public string optional_agreements_status { get; set; } = "";
        public int os { get; set; }
        public int parent_certificate_status { get; set; }
        public string parent_certificate_status_expiration { get; set; } = "";
        public ParentControlMap parent_control_map { get; set; } = null!;
        public int qr_code_ret { get; set; }
        public int realname_auth_status { get; set; }
        public string region { get; set; } = "";
        public int ret { get; set; }
        public string status_ts { get; set; } = "";
        public string tokenized_timer_expire { get; set; } = "";
        public TokenizedTimerExpireMap tokenized_timer_expire_map { get; set; } = null!;
        public string ts { get; set; } = "";
        public int ui_type { get; set; }
        public string uid_status { get; set; } = "";
        public string uid_status_msg { get; set; } = "";
        public int uid_status_ret { get; set; }
        public int voice_control_status { get; set; }
    }

    public class NeedNotifyRsp
    {
        public string game_sacc_openid { get; set; } = "";
        public string game_sacc_uid { get; set; } = "";
        public bool has_game_sacc_openid { get; set; }
        public bool has_game_sacc_uid { get; set; }
        public bool has_li_openid { get; set; }
        public bool has_li_uid { get; set; }
        public int is_receive_email { get; set; }
        public int is_receive_email_in_night { get; set; }
        public string li_openid { get; set; } = "";
        public string li_uid { get; set; } = "";
        public bool need_notify { get; set; }
        public string user_agreed_game_dma { get; set; } = "";
        public string user_agreed_game_nda { get; set; } = "";
        public string user_agreed_game_pp { get; set; } = "";
        public string user_agreed_game_tos { get; set; } = "";
        public string user_agreed_li_dt { get; set; } = "";
        public string user_agreed_li_pp { get; set; } = "";
        public string user_agreed_li_tos { get; set; } = "";
    }

    public class ParentControlMap
    {
    }
    public class LoginEndpoint1Res : IntlApiResponse
    {
        public string birthday { get; set; } = "";
        public ChannelInfo channel_info { get; set; } = null!;
        public string del_account_info { get; set; } = "";
        public int del_account_status { get; set; }
        public int del_li_account_status { get; set; }
        public string email { get; set; } = "";
        public ExtraJson extra_json { get; set; } = null!;
        public int first_login { get; set; }
        public int gender { get; set; }
        public bool need_name_auth { get; set; }
        public string openid { get; set; } = "";
        public string pf { get; set; } = "";
        public string pf_key { get; set; } = "";
        public string picture_url { get; set; } = "";
        public string reg_channel_dis { get; set; } = "";
        public string token { get; set; } = "";
        public int token_expire_time { get; set; }
        public string uid { get; set; } = "";
        public string user_name { get; set; } = "";
    }
    public class LoginEndpoint2Req
    {
        public DeviceInfo device_info { get; set; } = new();
        public string extra_json { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string password { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public int support_captcha { get; set; }
    }
    public class LoginEndpoint2Res : IntlApiResponse
    {
        public long expire { get; set; }
        public string token { get; set; } = "";
        public string uid { get; set; } = "";
    }

    public class RegisterEPReq
    {
        public DeviceInfo device_info { get; set; } = new();
        public string verify_code { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string phone_area_code { get; set; } = "";
        public string password { get; set; } = "";
        public string user_name { get; set; } = "";
        public string birthday { get; set; } = "";
        public string region { get; set; } = "";
        public string user_lang_type { get; set; } = "";
        public string extra_json { get; set; } = "";
    }
    public class SendCodeRequest
    {
        public DeviceInfo device_info { get; set; } = new();
        public string extra_json { get; set; } = "";
        public string account { get; set; } = "";
        public int account_type { get; set; }
        public string phone_area_code { get; set; } = "";
        public int code_type { get; set; }
        public int support_captcha { get; set; }
    }
    public class IntlApiResponse
    {
        public required string msg { get; set; } = "";
        public required int ret { get; set; }
        public required string seq { get; set; } = "";
    }
    public class ContentList
    {
        public string app_content_id { get; set; } = "";
        public string content { get; set; } = "";
        public string extra_data { get; set; } = "";
        public int id { get; set; }
        public string lang_type { get; set; } = "";
        public List<PictureList> picture_list { get; set; } = [];
        public string title { get; set; } = "";
        public int update_time { get; set; }
    }

    public class PictureList
    {
        public string extra_data { get; set; } = "";
        public string hash { get; set; } = "";
        public string redirect_url { get; set; } = "";
        public string url { get; set; } = "";
    }
    public class IntlNotice
    {
        public string app_id { get; set; } = "";
        public string app_notice_id { get; set; } = "";
        public string area_list { get; set; } = "";
        public List<ContentList> content_list { get; set; } = [];
        public int end_time { get; set; }
        public string extra_data { get; set; } = "";
        public int id { get; set; }
        public List<PictureList> picture_list { get; set; } = [];
        public int start_time { get; set; }
        public int status { get; set; }
        public int update_time { get; set; }
    }
    public class IntlNoticeListResponse : IntlApiResponse
    {
        public List<IntlNotice> notice_list { get; set; } = [];
    }

    public enum NoticeType
    {
        None = 0,
        Title = 1,
        Daily = 2,
        Event = 3,
        System = 4,
        Emergency = 5,
        PollData = 6
    }

    public class GetMinorcerStatusReq
    {
        public string openid { get; set; } = "";
        public bool support_os_separate_adult_age { get; set; } = true;
        public int mode { get; set; } = 1;
    }
    public class GetMinorcerStatusRes : IntlApiResponse
    {
        // don't really care about it
    }
    public class SimpleRequest
    {
        public string token { get; set; } = "";
        public string openid { get; set; } = "";
    }
    public class SimpleRequestAccount
    {
        public string token { get; set; } = "";
        public string uid { get; set; } = "";
    }
    public class ChannelInfo2
    {
        public string avatar_url { get; set; } = "";
        public string birthday { get; set; } = "";
        public string email { get; set; } = "";
        public int is_receive_email { get; set; }
        public string lang_type { get; set; } = "";
        public int last_login_time { get; set; }
        public string nick_name { get; set; } = "";
        public string phone { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public string region { get; set; } = "";
        public string register_account { get; set; } = "";
        public int register_account_type { get; set; }
        public int register_time { get; set; }
        public string seq { get; set; } = "";
        public string uid { get; set; } = "";
        public string user_name { get; set; } = "";
        public int username_pass_verify { get; set; }
    }
    public class BindList
    {
        public ChannelInfo2 channel_info { get; set; } = new();
        public int channelid { get; set; }
        public string email { get; set; } = "";
        public int is_link { get; set; }
        public string picture_url { get; set; } = "";
        public string user_name { get; set; } = "";
    }
    public class GetProfileResponse : IntlApiResponse
    {
        public List<BindList> bind_list { get; set; } = [];
        public string birthday { get; set; } = "";
        public string email { get; set; } = "";
        public int gender { get; set; }
        public string picture_url { get; set; } = "";
        public string user_name { get; set; } = "";
    }
    public class AccountGetUserInfoResponse : IntlApiResponse
    {
        public int account_type { get; set; }
        public string appid { get; set; } = "";
        public string avatar_url { get; set; } = "";
        public string birthday { get; set; } = "";
        public string email { get; set; } = "";
        public int expire { get; set; }
        public int is_receive_email { get; set; }
        public int is_receive_email_in_night { get; set; }
        public int is_receive_video { get; set; }
        public string lang_type { get; set; } = "";
        public string nick_name { get; set; } = "";
        public string phone { get; set; } = "";
        public string phone_area_code { get; set; } = "";
        public string privacy_policy { get; set; } = "";
        public int privacy_update_time { get; set; }
        public string region { get; set; } = "";
        public string terms_of_service { get; set; } = "";
        public int terms_update_time { get; set; }
        public string uid { get; set; } = "";
        public string user_agreed_dt { get; set; } = "";
        public string user_agreed_pp { get; set; } = "";
        public string user_agreed_tos { get; set; } = "";
        public string user_name { get; set; } = "";
        public int username_pass_verify { get; set; }
    }
    public class AuthData
    {
        public int ret { get; set; }
        public string msg { get; set; } = "";
        public int method_id { get; set; }
        public int ret_code { get; set; }
        public string ret_msg { get; set; } = "";
        public string extra_json { get; set; } = "";
        public string openid { get; set; } = "";
        public int token_expire_time { get; set; }
        public int first_login { get; set; }
        public string reg_channel_dis { get; set; } = "";
        public string user_name { get; set; } = "";
        public string picture_url { get; set; } = "";
        public bool need_name_auth { get; set; }
        public string channel_info { get; set; } = "";
        public string bind_list { get; set; } = "";
        public string confirm_code { get; set; } = "";
        public int confirm_code_expire_time { get; set; }
        public int channelid { get; set; }
        public string token { get; set; } = "";
        public int gender { get; set; }
        public string birthday { get; set; } = "";
        public string pf { get; set; } = "";
        public string pf_key { get; set; } = "";
        public string legal_doc { get; set; } = "";
        public string email { get; set; } = "";
        public int del_account_status { get; set; }
        public string del_account_info { get; set; } = "";
        public int del_li_account_status { get; set; }
        public string transfer_code { get; set; } = "";
        public int transfer_code_expire_time { get; set; }
        public string channel { get; set; } = "";
        public string link_li_token { get; set; } = "";
        public string link_li_uid { get; set; } = "";
        public string oauth_code { get; set; } = "";
        public int user_status { get; set; }
    }
    public class RepoManifest
    {
        public string file_url { get; set; } = "";
    }
    public class RepoData
    {
        public string cdn_root { get; set; } = "";
        public int chunk_encrypt_flag { get; set; }
        public List<RepoManifest> manifest_files { get; set; } = [];
    }
    public class RepoAccessData
    {
        public string encrytion_key { get; set; } = "";
        public string manifest_encrytion_key { get; set; } = "";
        public int encrytion_algorithm_id { get; set; }
        public int manifest_encrytion_algorithm_id { get; set; }
    }
    public class VersionInfo
    {
        public string version_id { get; set; } = "";
        public string version_name { get; set; } = "";
        public string installer_size { get; set; } = "";
        public string installed_size { get; set; } = "";
        public string publish_time_in_unix { get; set; } = "";
        [JsonConverter(typeof(JsonStringToObjectConverter<List<RepoData>>))]
        public List<RepoData> cos_repo_files { get; set; } = new();
        [JsonConverter(typeof(JsonStringToObjectConverter<List<RepoAccessData>>))]
        public List<RepoAccessData> cos_access_info { get; set; } = new();
    }

    public class LauncherVersion
    {
        public VersionInfo version_info { get; set; } = new();
        public VersionInfo launcher_version_info { get; set; } = new();
    }
    [JsonSourceGenerationOptions(WriteIndented = false)]
    [JsonSerializable(typeof(LoginEndpoint2Req))]
    [JsonSerializable(typeof(LoginEndpoint2Res))]
    [JsonSerializable(typeof(LoginEndpoint1Req))]
    [JsonSerializable(typeof(LoginEndpoint1Res))]
    [JsonSerializable(typeof(GetMinorcerStatusReq))]
    [JsonSerializable(typeof(GetMinorcerStatusRes))]
    [JsonSerializable(typeof(SimpleRequest))]
    [JsonSerializable(typeof(GetProfileResponse))]
    [JsonSerializable(typeof(SimpleRequestAccount))]
    [JsonSerializable(typeof(AccountGetUserInfoResponse))]
    [JsonSerializable(typeof(AuthData))]
    [JsonSerializable(typeof(CoreInfo))]
    [JsonSerializable(typeof(LauncherVersion))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
