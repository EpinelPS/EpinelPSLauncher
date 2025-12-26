/*
 * Helper DLL to enable Linux support
 * Shift Up, please add official linux support!
 * 
 * File licensed under GPL v3
*/

#include "pch.h"
#include <windows.h>
#include <cstdint>

enum AntiLinuxResult
{
	ACE_OK = 0,
	ACE_INVALID_ARGUMENT = 1,
	ACE_DEPLOYMENT_ERROR = 2,
	ACE_NOT_SUPPORTED = 3,
	ACE_INTERNAL_ERROR = 4,
	ACE_ILLEGAL_INIT = 5,
	ACE_NO_LAUNCHER = 6,
	ACE_CONFIG_ERROR = 7,
	ACE_SAFE_ERROR = 8,
	ACE_ROLE_ERROR = 9,
	ACE_ILLEGAL_LOG_ON = 100
};

// Nothing really needs to be done here.
BOOL APIENTRY DllMain(HMODULE hModule,
	DWORD  ul_reason_for_call,
	LPVOID lpReserved
)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}


void InitAceClient()
{
	// Not called?
}

void InitAceClient0()
{
	// Not called?
}
void InitAceClient2()
{
	// Not called?
}
void InitAceClient3()
{
	// Not called in GameAssembly?, but called internally in InitAceClient4
}

/*
 * V4
*/

struct System_String_o {

};

struct AceSdk_ClientInitInfo_Fields {
	DWORD first_process_pid;
	DWORD current_process_role_id;
	struct System_String_o* base_dat_path;
};


struct Client4 {
	Client4* handle;

	AntiLinuxResult(*login) (_In_ Client4*, void* account_info);
	AntiLinuxResult(*tick) (_In_ Client4*);
	AntiLinuxResult(*logoff) (_In_ Client4*);
	AntiLinuxResult(*exit) (_In_ Client4*);
	void*(*get_optional_interface) (_In_ Client4*);
};

struct AceSdk_ClientOptional_WrappedOptional_Fields {
	intptr_t opt;
	struct AceSdk_ClientOptional_GetTssAntibotRoutine_o* get_tss_antibot;
	struct AceSdk_ClientOptional_SetExitingCallbackRoutine_o* set_exiting_callback;
	struct AceSdk_ClientOptional_GetCustomInterfaceRoutine_o* get_custom_interface;
};
struct AceSdk_TssAntibot_WrappedTssAntibot_Fields {
	intptr_t antibot;
	struct AceSdk_TssAntibot_DeprecatedRoutine_o* deprecated;
	struct AceSdk_TssAntibot_GetReportAntiDataRoutine_o* get_report_anti_data;
	struct AceSdk_TssAntibot_DelReportAntiDataRoutine_o* del_report_anti_data;
	struct AceSdk_TssAntibot_OnRecvAntiDataRoutine_o* on_recv_anti_data;
	struct AceSdk_TssAntibot_DeprecatedRoutine2_o* deprecated2;
};

// Static structures
static Client4 pclient;

static AceSdk_ClientOptional_WrappedOptional_Fields poptionalInterface;
static AceSdk_TssAntibot_WrappedTssAntibot_Fields AntibotInterface;


// Data
static void* ExitCallback;


static AceSdk_TssAntibot_WrappedTssAntibot_Fields* AceClient_GetTssAntibot(void* ptr)
{
	// instantly crash game if these methods are called
	AntibotInterface.del_report_anti_data = (AceSdk_TssAntibot_DelReportAntiDataRoutine_o*)1;
	AntibotInterface.get_report_anti_data = (AceSdk_TssAntibot_GetReportAntiDataRoutine_o*)2;
	AntibotInterface.on_recv_anti_data = (AceSdk_TssAntibot_OnRecvAntiDataRoutine_o*)3;

	AntibotInterface.deprecated = (AceSdk_TssAntibot_DeprecatedRoutine_o*)4;
	AntibotInterface.deprecated2 = (AceSdk_TssAntibot_DeprecatedRoutine2_o*)5;

	return &AntibotInterface;
}

static void* AceClient_GetCustomInterface(void* ptr, int type)
{
	// no clue what this is, does not appear to be used
	return NULL;
}

static void AceClient_SetExitCb(Client4*, void* exitCb, void* ctx)
{
	ExitCallback = exitCb;
}

/// <summary>
/// Construct optional inteface structure
/// </summary>
/// <param name="ptr"></param>
/// <returns></returns>
static void* AceClient_Opt(Client4*)
{
	// get pointer to the optional interface
	poptionalInterface.get_tss_antibot = (AceSdk_ClientOptional_GetTssAntibotRoutine_o*)AceClient_GetTssAntibot;
	poptionalInterface.set_exiting_callback = (AceSdk_ClientOptional_SetExitingCallbackRoutine_o*)AceClient_SetExitCb;
	poptionalInterface.get_custom_interface = (AceSdk_ClientOptional_GetCustomInterfaceRoutine_o*)AceClient_GetCustomInterface;

	return (void*)&poptionalInterface;
}

static AntiLinuxResult AceClient_Logon(Client4* acePtr, void* account_info)
{
	return ACE_OK;
}

static AntiLinuxResult AceClient_Logoff(Client4*)
{
	// does not appear to be called
	return ACE_OK;
}

static AntiLinuxResult AceClient_Tick(Client4*)
{
	// Called in AceClient.tick
	return ACE_OK;
}

AntiLinuxResult InitAceClient4(AceSdk_ClientInitInfo_Fields* info, ULONG flags, Client4** client)
{
	// used in older verisons
	pclient.handle = &pclient; // client handle
	pclient.login = AceClient_Logon;
	pclient.logoff = AceClient_Logoff;
	pclient.get_optional_interface = AceClient_Opt;
	pclient.tick = AceClient_Tick;

	*client = &pclient;
	return ACE_OK;
}

/*
 * V5
*/

typedef void* ANTILINUXHANDLE;
#define AL_HANDLE (ANTILINUXHANDLE)123

struct Client5FullPacket
{

};

struct Client5FeaturePacket
{

};

struct Client5Feature
{

};

struct Client5 {
	Client5* handle;
	void* unused;

	AntiLinuxResult(*logout) (_In_ Client5*);
	AntiLinuxResult(*cleanup) (_In_ Client5*);
	AntiLinuxResult(*get_packet) (_In_ Client5*, _Inout_ Client5FullPacket*);
	AntiLinuxResult(*on_packet_rx) (_In_ Client5*, BYTE* data, int len);
	AntiLinuxResult(*login) (_In_ Client5*, LPCSTR account, int type, UINT worldId, LPCSTR ticket);
	AntiLinuxResult(*get_light_packet) (_In_ Client5*, _Inout_ Client5FeaturePacket*);
	AntiLinuxResult(*on_light_packet_rx) (_In_ Client5*, Client5Feature*);
};

static Client5 client5;

AntiLinuxResult Antilinux5_Logout(Client5* handle)
{
	return ACE_OK;
}
AntiLinuxResult Antilinux5_Cleanup(Client5* handle)
{
	return ACE_OK;
}

AntiLinuxResult Antilinux5_GetPacket(Client5* handle, _Inout_ Client5FullPacket*)
{
	MessageBoxA(NULL, "Warning: Networking related GetPacket function called.\n\nAnticheat is in use.", "Warning", MB_ICONWARNING);
	return ACE_OK;
}

AntiLinuxResult Antilinux5_OnPacketRx(_In_ Client5*, BYTE* data, int len)
{
	MessageBoxA(NULL, "Warning: Networking related OnPacketRx function called.\n\nAnticheat is in use.", "Warning", MB_ICONWARNING);
	return ACE_OK;
}

AntiLinuxResult Antilinux5_Login(_In_ Client5*, LPCSTR account, int type, UINT worldId, LPCSTR ticket)
{
	return ACE_OK;
}

AntiLinuxResult Antilinux5_GetLightPacket(_In_ Client5*, _Inout_ Client5FeaturePacket*)
{
	MessageBoxA(NULL, "Warning: Networking related GetLightPacket function called.\n\nAnticheat is in use.", "Warning", MB_ICONWARNING);
	return ACE_OK;
}

AntiLinuxResult Antilinux5_OnLightPacketRx(_In_ Client5*, Client5Feature*)
{
	MessageBoxA(NULL, "Warning: Networking related OnLightPacketRx function called.\n\nAnticheat is in use.", "Warning", MB_ICONWARNING);
	return ACE_OK;
}

AntiLinuxResult InitAceClient5(_In_ int version, _In_opt_ void* optional, _Out_ Client5** client)
{
	client5.handle = &client5;
	client5.logout = Antilinux5_Logout;
	client5.cleanup = Antilinux5_Cleanup;
	client5.get_packet = Antilinux5_GetPacket;
	client5.on_packet_rx = Antilinux5_OnPacketRx;
	client5.login = Antilinux5_Login;
	client5.get_light_packet = Antilinux5_GetLightPacket;
	client5.on_light_packet_rx = Antilinux5_OnLightPacketRx;

	*client = &client5;
	return ACE_OK;
}

void NullExportFunction()
{
	// Ordinal #6: Does not appear to be called.
}
