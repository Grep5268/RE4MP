// RE4MP.cpp : This file contains the 'main' function. Program execution begins and ends there.
// NOTE OVERWRITING CODE REQUIRES FULL ADDRESS (DO NOT INCLUDE BASE ADDRESS OF RAM)
//

#include <Windows.h>
#include <iostream>
#include <string>
#include <stdio.h>
#include <vector>
#include "RE4MP.h"

DWORD procID; // A 32-bit unsigned integer, DWORDS are mostly used to store Hexadecimal Addresses
HANDLE handle;

typedef __int64(__thiscall* fn_cSubChar_movePos)(int cSubCharPtr, std::vector<float>* toPos, float spd);
fn_cSubChar_movePos cSubChar_movePos;
//std::vector<float> *pos = new std::vector<float>{ -41085.26172, 15.20379639, -3521.290771 };
//cSubChar_movePos(*(int*)(base_addr + 0x857060), pos, spd); // subchar pointer dereferenced, function applies offset

typedef __int64(__fastcall* fn_cSubChar_move)(int cSubCharPtr);
fn_cSubChar_move cSubChar_move;

DWORD WINAPI MainThread(LPVOID param) {

    DWORD base_addr = (DWORD)GetModuleHandleA(0);
    HWND hwnd = FindWindowA(NULL, "Resident Evil 4"); // HWND (Windows window) by Window Name
    
    GetWindowThreadProcessId(hwnd, &procID); // Getting our Process ID, as an ex. like 000027AC
    handle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, procID); // Opening the Process with All Access

    HookFunctions(base_addr);

    CodeInjection(base_addr);
    //std::string wstr = std::to_string(base_addr + 0x3567e0);
    
    while (true) {
        if (GetAsyncKeyState(VK_F5)) {
            float pos[3] = { -35103.39844, 34.03607178, -3051.361572 };
            MoveSubChar(base_addr, pos);
        }

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
    default:
        break;
    }

    return TRUE;
}

void OverwriteBytes(DWORD addr, char* bytes, int len)
{
    WriteProcessMemory(handle, (LPVOID)(addr), bytes, len, NULL);
}

void HookFunctions(DWORD base_addr) 
{
    cSubChar_movePos = (fn_cSubChar_movePos)(base_addr + 0x3567e0); //0x7567e0
    cSubChar_move = (fn_cSubChar_move)(base_addr + 0x361a70);
}

void CodeInjection(DWORD base_addr)
{
    // Overwrite cSubChar destination data writes (TODO set it to itself instead of all this jump nonsense)
    char bytes[0x5] = { 0xe9, 0x15, 0x01, 00, 00 }; //jmp    11a <_main+0x11a>
    OverwriteBytes((base_addr + 0x35e8f4), bytes, 5);
}

int* SubCharPointer(DWORD base_addr) 
{
    return (int*)(base_addr + 0x857060);
}

void MoveSubChar(DWORD base_addr, float* toPos)
{
    int* ptr = SubCharPointer(base_addr);
     *(float*)((*ptr) + 0x450) = toPos[0];
     *(float*)((*ptr) + 0x454) = toPos[1];
     *(float*)((*ptr) + 0x458) = toPos[2];
}