#pragma once
#include "hooks.h"

int HookedEmReadSearch(uint8_t npcId, void* data_addr, uint32_t malloc_size)
{
    if (npcId == 0x44)
    {
        npcId = 0x20;
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

    // Do something before calling the original function, such as logging or modifying arguments
    int result = EmReadSearch(npcId, data_addr, malloc_size);
    // Do something after the original function returns, such as modifying the result or logging
    return result;
}

void DetourFunctions(DWORD base_addr)
{
    // detour
    //DetourTransactionBegin();
    //DetourUpdateThread(GetCurrentThread());
    //EmReadSearch = (fn_EmReadSearch)(base_addr + 0x2AE8D0);
   // DetourAttach(&(PVOID&)EmReadSearch, HookedEmReadSearch);
   // DetourTransactionCommit();
}