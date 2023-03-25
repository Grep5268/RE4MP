#pragma once
#include "hooks.h"
#include "Cache.h"
#include <cmath>

bool testFlag = false;

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

void __fastcall HookedCActionMoveAttack(void* cAction, void* notUsed, void* cAnalysis, void* cRoutine)
{
    if (playerTwoPtr != nullptr && (int)cAction == ((int)playerTwoPtr + 0x714))
    {
        // todo mess with this to make it good
        *(byte*)((int)cAction + 0xc) = 2; //rno1_C
        *(int*)((int)cRoutine + 0xa8) = (int)SubCharPointer(base_addr); // change to enemy hit?
        *(int*)((int)cRoutine + 0x14) = (int)SubCharPointer(base_addr); // change to enemy hit?
        *(int*)((int)cRoutine + 0xba) = 1; // shoot type>>??
    }

    cAction_moveAttack(cAction, cAnalysis, cRoutine);
    
    if (playerTwoPtr != nullptr && (int)cAction == ((int)playerTwoPtr + 0x714))
    {
        cRoutine_moveWepFire(cRoutine);
    }

    return;
}


void DetourFunctions(DWORD base_addr)
{
    DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());

    CSubLuisThink = (fn_cSubLuis_think)(base_addr + 0x04e8af0);
    DetourAttach(&(PVOID&)CSubLuisThink, HookedCSubLuisThink);

    RouteCkToPos = (fn_RouteCkToPos)(base_addr + 0x02B2950);
    DetourAttach(&(PVOID&)RouteCkToPos, HookedRouteCkToPos);

    cAction_moveAttack = (fn_cAction_moveAttack)(base_addr + 0x4e4d70);
    DetourAttach(&(PVOID&)cAction_moveAttack, HookedCActionMoveAttack);
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
