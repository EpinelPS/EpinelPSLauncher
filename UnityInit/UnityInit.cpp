/*
 * Helper EXE file to start Unity
*/

#include <windows.h>

// TODO: is this signature correct?
typedef int (*UnityMainFN) (HINSTANCE, LPSTR);

int WinMain(
    _In_           HINSTANCE hInstance,
    _In_opt_       HINSTANCE hPrevInstance,
    _In_           LPSTR     lpCmdLine,
    _In_           int       nShowCmd
)
{
    HMODULE unityEngine = LoadLibraryW(L"UnityPlayer.dll");

    if (!unityEngine)
    {
        MessageBoxW(nullptr, L"Failed to load UnityPlayer.dll. Ensure that this executable is in the game folder.", L"Error", MB_ICONERROR);
        return -1;
    }

    UnityMainFN fn = (UnityMainFN)GetProcAddress(unityEngine, "UnityMain");
    if (!fn)
    {
        MessageBoxW(nullptr, L"Failed to locate UnityMain in UnityPlayer.dll", L"Error", MB_ICONERROR);
        return -1;
    }

    return fn(hInstance, lpCmdLine);
}
