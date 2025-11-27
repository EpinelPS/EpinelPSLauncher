// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <stdio.h>
#include <iostream>
#include <vector>

LPSTR loginData = NULL;
LPSTR resourcePath = NULL;
int launcherFormat = 1;


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

class GameString {
public:
	GameString(LPCSTR str, int len)
	{
		if (strlen(str) > len)
			len = (int)strlen(str);
		theString = (LPSTR)HeapAlloc(GetProcessHeap(), 0, len + 1);
		if (theString != nullptr)
			strcpy(theString, str);

		// manually clear fields
		data = 0;
		data2 = 0;
		data3 = 0;
		data4 = 0;
		data5 = 0;
		data6 = 0;
	}
	virtual ~GameString() {
		// shouldnt be called
	}
	virtual void vtable01() {
		// shouldnt be called
	}
	virtual void vtable02() {
		// shouldnt be called
	}


	LPSTR theString;
	int data;
	int data2;
	int data3;
	int data4;
	int data5;
	int data6;
};
class CPlayer
{
	// TODO figure out how to remove unused functions
	virtual void vtable00()
	{
		printf("shouldnt be called: CPlayer::vtable00\n");
	}

	virtual int GetLoginData(GameString* str)
	{
		if (loginData)
		{
			void* newMem = HeapReAlloc(GetProcessHeap(), 0, (str)->theString, strlen(loginData) + 1);
			if (newMem)
			{
				(str)->theString = (LPSTR)newMem;
				strcpy((str)->theString, loginData);
			}
			else
			{
				printf("!!!failed to realloc memory!!!\n");
			}
		}
		else
		{
			printf("login data missing\n");
			strcpy((str)->theString, "{}");
		}
		return 0;
	}


	virtual int GetComplianceRegion(GameString* str)
	{
		const char* language = "English";
		void* newMem = HeapReAlloc(GetProcessHeap(), 0, (str)->theString, strlen(language) + 1);
		if (newMem)
		{
			(str)->theString = (LPSTR)newMem;
			strcpy((str)->theString, language);
		}
		else
		{
			printf("!!!failed to realloc memory!!!\n");
		}
		return 0;
	}
};
class CGame
{
	virtual ~CGame() {
		printf("called ~CGame\n");
	}

	virtual int vtable01()
	{
		printf("vtable01 end\n");
		return 0;
	}

	virtual int GetPlatformLanguageCode(int type, GameString* str)
	{
		const char* language = "English"; // TODO: Dont hardcode this
		void* newMem = HeapReAlloc(GetProcessHeap(), 0, (str)->theString, strlen(language) + 1);

		if (newMem)
		{
			(str)->theString = (LPSTR)newMem;
			strcpy((str)->theString, language);

		}
		else {
			printf("memory allocation failure!!!");
		}

		return 0;
	}
	virtual int SetGameLanguageCode(int type, GameString* str)
	{
		return 0;
	}
	virtual int vtable03()
	{
		printf("3 end\n");
		return 0;
	}
	virtual int v4()
	{
		printf("4 end\n");
		return 0;
	}
	virtual int v5()
	{
		printf("5 end\n");
		return 0;
	}
	virtual int v6()
	{
		printf("6 end\n");
		return 0;
	}

	virtual int v7()
	{
		printf("7 end\n");
		return 0;
	}

	virtual int v8()
	{
		printf("8 end\n");
		return 0;
	}

	virtual int GetGameResourcePath(GameString* str) {
		void* newMem = HeapReAlloc(GetProcessHeap(), 0, (str)->theString, strlen(resourcePath) + 1);
		if (newMem)
		{
			(str)->theString = (LPSTR)newMem;
			strcpy((str)->theString, resourcePath);
		}
		else
		{
			printf("!!!failed to realloc memory!!!\n");
		}
		return 0;
	}
};

class CFactory {
public:
	CPlayer* player;
	CGame* game;
	CFactory(int launcherFormat)
	{
		player = new CPlayer();
		game = new CGame();
	}
	virtual CPlayer* GetPlayer()
	{
		return player;
	}
	virtual CGame* GetGame()
	{
		return game;
	}
};

class CLauncherPipe {
public:
	int Connect()
	{
		const DWORD bufferSize = 4096;

		// communicate to the .NET launcher helper using a named pipe
		hPipe = CreateFile(
			L"\\\\.\\pipe\\goodpipe",
			GENERIC_READ,
			0,
			NULL,
			OPEN_EXISTING,
			0,
			NULL
		);

		if (hPipe == INVALID_HANDLE_VALUE) {
			std::cout << "Failed to connect to pipe. Error: " << GetLastError() << std::endl;
			return 1;
		}

		char tempBuffer[bufferSize];
		while (true) {
			if (!ReadFile(hPipe, tempBuffer, bufferSize, &bytesRead, NULL) || bytesRead == 0) {
				break; // Exit loop on error or no data
			}
			buffer.insert(buffer.end(), tempBuffer, tempBuffer + bytesRead);
		}


		CloseHandle(hPipe);

		return 0;
	}

	int ParseData()
	{
		if (buffer.empty())
		{
			printf("CLauncherPipe: no data!\n");
			return -1;
		}

		char* bufferPtr = buffer.data();

		// read vtable format byte.
		// 0: No resource path function
		// 1: Has resource path function

		launcherFormat = *bufferPtr;
		if (launcherFormat != 0 && launcherFormat != 1)
		{
			printf("CLauncherPipe: invalid communication format!\n");
			return -2;
		}
		printf(" - Game launcher protocol version: %d\n", launcherFormat);

		bufferPtr++;

		// read resource path string
		resourcePath = _strdup(bufferPtr);
		printf(" - Resource path %s\n", resourcePath);
		bufferPtr += strlen(resourcePath) + 1;

		// read login info json string
		loginData = _strdup(bufferPtr);
		printf(" - Login info %s\n", loginData);
		bufferPtr += strlen(loginData) + 1;

		return 0;
	}

private:
	HANDLE hPipe;
	DWORD bytesRead;
	std::vector<char> buffer;
};


extern "C"
{
	__declspec(dllexport) int SailInitialize(int id, int b, int c)
	{
		//AllocConsole();
		//freopen("CONOUT$", "w", stdout);

		printf("SailInitialize: GameID: %d args: %x other: %x\n", id, b, c);

		CLauncherPipe pipe;

		int result = pipe.Connect();
		if (result != 0) return result;

		result = pipe.ParseData();
		if (result != 0) return result;

		return 0;
	}

	__declspec(dllexport) CFactory* SailFactory()
	{
		printf("SailFactory\n");
		static CFactory* factory = NULL;
		if (!factory)
		{
			factory = new CFactory(launcherFormat);
		}

		return factory;
	}
}
