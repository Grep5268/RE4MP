// RE4MP.cpp : This file contains the 'main' function. Program execution begins and ends there.
// NOTE OVERWRITING CODE REQUIRES FULL ADDRESS (DO NOT INCLUDE BASE ADDRESS OF RAM)
//

#include <Windows.h>
#include "detours.h"
#include <iostream>
#include <string>
#include <stdio.h>
#include <vector>
#include "RE4MP.h"
#include "hooks.h"
#include "Re4Detours.h"

DWORD procID; // A 32-bit unsigned integer, DWORDS are mostly used to store Hexadecimal Addresses
HANDLE handle;

DWORD WINAPI MainThread(LPVOID param) {

    base_addr = (DWORD)GetModuleHandleA(0);
    HWND hwnd = FindWindowA(NULL, "Resident Evil 4"); // HWND (Windows window) by Window Name
    
    GetWindowThreadProcessId(hwnd, &procID); // Getting our Process ID, as an ex. like 000027AC
    handle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, procID); // Opening the Process with All Access

    HookFunctions(base_addr);
    DetourFunctions(base_addr);
    CodeInjection(handle, base_addr);
    //std::string wstr = std::to_string(base_addr + 0x3567e0);
    
    while (true) {

        
        if (GetAsyncKeyState(VK_F5)) {
            // playerTwoPtr + 0x94
            playerTwoPtr = (int*)cManager_cEm__createBack(GetEmMgrPointer(base_addr), 0x4);
        
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
            }
            */

        }

        // cSubChar manual control
        float moveFactor = 20.0;
        if (GetAsyncKeyState(VK_NUMPAD0)) {
            float* pos = GetPlayerPosition(base_addr);
            MoveSubChar(base_addr, pos);
        }

        if (GetAsyncKeyState(VK_UP) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharDestinationPos(base_addr);
            pos[2] += moveFactor;
            MoveSubChar(base_addr, pos);
        }
        else if (GetAsyncKeyState(VK_DOWN) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharDestinationPos(base_addr);
            pos[2] -= moveFactor;
            MoveSubChar(base_addr, pos);
        }

        if (GetAsyncKeyState(VK_LEFT) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharDestinationPos(base_addr);
            pos[0] += moveFactor;
            MoveSubChar(base_addr, pos);
        }
        else if (GetAsyncKeyState(VK_RIGHT) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharDestinationPos(base_addr);
            pos[0] -= moveFactor;
            MoveSubChar(base_addr, pos);
        }

        // y pos
        if (GetAsyncKeyState(VK_F6) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharPos(base_addr);
            pos[1] += moveFactor;
            MoveSubChar(base_addr, pos);
        }
        else if (GetAsyncKeyState(VK_F7) && playerTwoPtr != nullptr) {
            float* pos = GetSubCharPos(base_addr);
            pos[1] -= moveFactor;
            MoveSubChar(base_addr, pos);
        }

        // exit scenario
        if (GetAsyncKeyState(VK_END)) {
            break;
        }

        Sleep(10); //may cause lag
    }

    FreeLibraryAndExitThread((HMODULE) param, 0);
    return 0;
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD reason, LPVOID l)
{
        switch (reason)
        {
        case DLL_PROCESS_ATTACH:
            CreateThread(NULL, NULL, MainThread, hModule, NULL, NULL);
            break;
        case DLL_PROCESS_DETACH:
        default:
            break;
        }

    return TRUE;
}

