// RE4MP.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <Windows.h>
#include <iostream>
#include <string>
#include <stdio.h>
#include <vector>
//#define base_addr 0x00400000

typedef __int64(__thiscall* fn_cSubChar_movePos)(int cSubCharPtr, std::vector<float>* toPos, float spd);
fn_cSubChar_movePos cSubChar_movePos;

DWORD WINAPI MainThread(LPVOID param) {

    DWORD base_addr = (DWORD)GetModuleHandleA(0);
    cSubChar_movePos = (fn_cSubChar_movePos)(base_addr + 0x3567e0); //0x7567e0
    std::string wstr = std::to_string(base_addr + 0x3567e0);
    
    while (true) {
        if (GetAsyncKeyState(VK_F5)) {
            std::vector<float> *pos = new std::vector<float>{ -41085.26172, 15.20379639, -3521.290771 };
            float spd = 75;
            
            cSubChar_movePos(*(int*)(base_addr + 0x857060), pos, spd); // subchar pointer dereferenced, function applies offset
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

