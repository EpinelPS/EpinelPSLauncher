/*
 * Helper DLL to enable Linux support
 * Shift Up, please add official linux support!
 * 
 * File licensed under GPL v3
*/

#include "pch.h"
#include <windows.h>
#include <cstdint>

enum ACE_RESULT
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

struct System_String_o {

};

struct AceSdk_ClientInitInfo_Fields {
	DWORD first_process_pid;
	DWORD current_process_role_id;
	struct System_String_o* base_dat_path;
};


struct AceSdk_AceClient_WrapperAceClient_Fields {
	intptr_t ace_client;
	struct AceSdk_AceClient_LogOnRoutine_o* log_on;
	struct AceSdk_AceClient_TickRoutine_o* tick;
	struct AceSdk_AceClient_LogOffRoutine_o* log_off;
	struct AceSdk_AceClient_ExitProcessRoutine_o* exit_process;
	struct AceSdk_AceClient_GetOptRoutine_o* get_optional_interface;
};

struct AceSdk_AceClient_WrapperAceClient_o {
	AceSdk_AceClient_WrapperAceClient_Fields fields;
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
static AceSdk_AceClient_WrapperAceClient_Fields pclient;

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

static void AceClient_SetExitCb(void* ptr, void* exitCb, void* ctx)
{
	ExitCallback = exitCb;
}

/// <summary>
/// Construct optional inteface structure
/// </summary>
/// <param name="ptr"></param>
/// <returns></returns>
static void* AceClient_Opt(void* ptr)
{
	// get pointer to the optional interface
	poptionalInterface.get_tss_antibot = (AceSdk_ClientOptional_GetTssAntibotRoutine_o*)AceClient_GetTssAntibot;
	poptionalInterface.set_exiting_callback = (AceSdk_ClientOptional_SetExitingCallbackRoutine_o*)AceClient_SetExitCb;
	poptionalInterface.get_custom_interface = (AceSdk_ClientOptional_GetCustomInterfaceRoutine_o*)AceClient_GetCustomInterface;

	return (void*)&poptionalInterface;
}

static ACE_RESULT AceClient_Logon(void* acePtr, void* account_info)
{
	return ACE_OK;
}

static void AceClient_Logoff(void* acePtr)
{
	// does not appear to be called
}

static void AceClient_Tick(void* acePtr)
{
	// Called in AceClient.tick
}

ACE_RESULT InitAceClient4(AceSdk_ClientInitInfo_Fields* info, ULONG flags, AceSdk_AceClient_WrapperAceClient_Fields** client)
{
	MessageBox(NULL, TEXT("InitAceClient4 is unimplemented"), TEXT("Error"), MB_OK);
	return ACE_OK;
}

ACE_RESULT InitAceClient5(AceSdk_ClientInitInfo_Fields* info, ULONG flags, AceSdk_AceClient_WrapperAceClient_Fields** client)
{
	pclient.ace_client = 123; // client handle
	pclient.log_on = (AceSdk_AceClient_LogOnRoutine_o*)AceClient_Logon;
	pclient.log_off = (AceSdk_AceClient_LogOffRoutine_o*)AceClient_Logoff;
	pclient.get_optional_interface = (AceSdk_AceClient_GetOptRoutine_o*)AceClient_Opt;
	pclient.tick = (AceSdk_AceClient_TickRoutine_o*)AceClient_Tick;

	*client = &pclient;
	return ACE_OK;
}

void NullExportFunction()
{
	// Ordinal #6: Does not appear to be called.
}
