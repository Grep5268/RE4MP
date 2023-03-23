#pragma once
#include "hooks.h"
#include "Cache.h"

void __fastcall HookedCSubLuisThink(void* luis, void* notUsed)
{

    // Do something before calling the original function, such as logging or modifying arguments
    CSubLuisThink(luis);
    *(int*)((int)luis + 0x718) = 2;
    *(int*)((int)luis + 0x71C) = 2;
    // Do something after the original function returns, such as modifying the result or logging
    return;
}

BOOL __cdecl HookedRouteCkToPos(void* cEm, float* pPos, float* pDest, uint32_t mode, float* pMax)
{
    HMODULE hUser32 = LoadLibraryA("user32.dll");

    if (hUser32 != NULL)
    {
        // Get the address of the MessageBox function
        FARPROC pMessageBox = GetProcAddress(hUser32, "MessageBoxA");

        if (pMessageBox != NULL && ((int)cEm == (int)playerTwoPtr))
        {
            // Call the MessageBox function
            ((void (WINAPI*)(HWND, LPCSTR, LPCSTR, UINT))pMessageBox)(NULL, std::to_string((int)cEm).c_str(), "Injected DLL", MB_OK);
        }

        // Free the library
        FreeLibrary(hUser32);
    }

    BOOL res = RouteCkToPos(cEm, pPos, pDest, mode, pMax);

    // Do something after the original function returns, such as modifying the result or logging
    return res;
}

void DetourFunctions(DWORD base_addr)
{
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());

    CSubLuisThink = (fn_cSubLuis_think)(base_addr + 0x04e8af0);
    DetourAttach(&(PVOID&)CSubLuisThink, HookedCSubLuisThink);

    RouteCkToPos = (fn_RouteCkToPos)(base_addr + 0x02B2950);
    DetourAttach(&(PVOID&)RouteCkToPos, HookedRouteCkToPos);
    DetourTransactionCommit();
}

/*
HMODULE hUser32 = LoadLibraryA("user32.dll");

if (hUser32 != NULL)
{
    // Get the address of the MessageBox function
    FARPROC pMessageBox = GetProcAddress(hUser32, "MessageBoxA");

    if (pMessageBox != NULL)
    {
        // Call the MessageBox function
        ((void (WINAPI*)(HWND, LPCSTR, LPCSTR, UINT))pMessageBox)(NULL, std::to_string(npcId).c_str(), "Injected DLL", MB_OK);
    }

    // Free the library
    FreeLibrary(hUser32);
}*/
