#pragma once
#include "hooks.h"
#include "Cache.h"
#include <cmath>

void __fastcall HookedCSubLuisThink(void* luis, void* notUsed)
{

    // Do something before calling the original function, such as logging or modifying arguments
    CSubLuisThink(luis);

    // set action type
    float* pos = playerTwoPos;
    float* cEmPos = GetCEmPos(playerTwoPtr);

    if (
        (std::fabs(pos[0] - cEmPos[0]) < 150)
        && (std::fabs(pos[2] - cEmPos[2]) < 150))
    {
        playerTwoActionType = 1;
    }
    else
    {
        playerTwoActionType = 2;
    }

    *(int*)((int)luis + 0x718) = playerTwoActionType;
    *(int*)((int)luis + 0x71C) = playerTwoActionType;

    // Do something after the original function returns, such as modifying the result or logging
    return;
}

BOOL __cdecl HookedRouteCkToPos(void* cEm, float* pPos, float* pDest, uint32_t mode, float* pMax)
{
    float* pos;

    if ((int)cEm == (int)playerTwoPtr)
    {
        pos = playerTwoPos; 
    }
    else {
        pos = pDest;
    }

    BOOL res = RouteCkToPos(cEm, pos, pDest, mode, pMax);

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
