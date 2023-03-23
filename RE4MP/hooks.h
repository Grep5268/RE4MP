#pragma once
typedef __int64(__thiscall* fn_cSubChar_movePos)(int cSubCharPtr, std::vector<float>* toPos, float spd);
fn_cSubChar_movePos cSubChar_movePos;

typedef __int64(__fastcall* fn_cSubChar_move)(int cSubCharPtr);
fn_cSubChar_move cSubChar_move;

typedef int(__thiscall* fn_cManager_cEm__createBack)(int* emMgr, uint32_t id);
fn_cManager_cEm__createBack cManager_cEm__createBack;

typedef int(__thiscall* fn_cManager_cEm__construct)(int* emMgr, int emMemory, uint8_t npcId);
fn_cManager_cEm__construct cManager_cEm__construct;

typedef int(__cdecl* fn_EmReadSearch)(uint8_t npcId, void* data_addr, uint32_t malloc_size);
fn_EmReadSearch EmReadSearch;

typedef void(__thiscall* fn_cSubLuis_think)(void* luis);
fn_cSubLuis_think CSubLuisThink;

typedef BOOL(__cdecl* fn_RouteCkToPos)(void* cEm, float* pPos, float* pDest, uint32_t mode, float* pMax);
fn_RouteCkToPos RouteCkToPos;

int* GetEmMgrPointer(DWORD base_addr)
{
    return (int*)(base_addr + 0x7fDB04);
}

int* GetEmMgrEmListPointer(DWORD base_addr)
{
    return (int*)(base_addr + 0x7fDB04 + 0x14);
}

int* PlayerPointer(DWORD base_addr)
{
    return (int*)(base_addr + 0x857054);
}

float* GetPlayerPosition(DWORD base_addr)
{
    int* ptr = PlayerPointer(base_addr);
    return (float*)((*ptr) + 0x94);
}

int* SubCharPointer(DWORD base_addr)
{
    return (int*)(base_addr + 0x857060);
}

float* GetSubCharPos(DWORD base_addr)
{
    int* ptr = SubCharPointer(base_addr);
    return (float*)((*ptr) + 0x94);
}

float* GetSubCharDestinationPos(DWORD base_addr)
{
    int* ptr = SubCharPointer(base_addr);
    return (float*)((*ptr) + 0x450);
}

char* GetSubCharGravityCheck(DWORD base_addr)
{
    int* ptr = SubCharPointer(base_addr);
    return (char*)((*ptr) + 0x2ce);
}

void MoveSubChar(DWORD base_addr, float* toPos)
{
    float* ptr = GetSubCharDestinationPos(base_addr);
    *GetSubCharGravityCheck(base_addr) = 2; // disable grabity
    ptr[0] = toPos[0];
    ptr[1] = toPos[1];
    ptr[2] = toPos[2];
}

void OverwriteBytes(HANDLE handle, DWORD addr, char* bytes, int len)
{
    WriteProcessMemory(handle, (LPVOID)(addr), bytes, len, NULL);
}

void HookFunctions(DWORD base_addr)
{
    cSubChar_movePos = (fn_cSubChar_movePos)(base_addr + 0x3567e0); //0x7567e0
    cSubChar_move = (fn_cSubChar_move)(base_addr + 0x361a70);
    cManager_cEm__createBack = (fn_cManager_cEm__createBack)(base_addr + 0x1b23f0);
}

void CodeInjection(HANDLE handle, DWORD base_addr)
{
    char twoNop[2] = { 0x90, 0x90 }; //nop
    char threeNop[3] = { 0x90, 0x90, 0x90 }; //nop
    char fiveNop[5] = { 0x90, 0x90, 0x90, 0x90, 0x90 }; //nop
    char sixNop[6] = { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 }; //nop

    // Disable Luis partner set and move
    OverwriteBytes(handle, (base_addr + 0x4e8a41), sixNop, 6);
    //OverwriteBytes(handle, (base_addr + 0x4ea021), fiveNop, 5);

    // Disable cSubChar setting partner location for movement
    /*OverwriteBytes((base_addr + 0x35e9fa), twoNop, 2);
    OverwriteBytes((base_addr + 0x35ea02), threeNop, 3);
    OverwriteBytes((base_addr + 0x35ea0b), threeNop, 3);

    OverwriteBytes((base_addr + 0x35e9cb), twoNop, 2);
    OverwriteBytes((base_addr + 0x35e9cd), threeNop, 3);
    OverwriteBytes((base_addr + 0x35e9d0), threeNop, 3);

    OverwriteBytes((base_addr + 0x35eb4c), fiveNop, 5); // Routing

    OverwriteBytes((base_addr + 0x35e9df), fiveNop, 5); // odd math funcs
    OverwriteBytes((base_addr + 0x35eb00), fiveNop, 5);
    */

    // spawn subChar everywhere
    //OverwriteBytes((base_addr + 0x2c520b), twoNop, 2);
}
