#pragma once
#include <Windows.h>

void HookFunctions(DWORD base_address);
void CodeInjection(DWORD base_addr);

int* GetEmMgrPointer(DWORD base_addr);
int* PlayerPointer(DWORD base_addr);
float* GetPlayerPosition(DWORD base_addr);

int* SubCharPointer(DWORD base_addr);
float* GetSubCharPos(DWORD base_addr);
float* GetSubCharDestinationPos(DWORD base_addr);
void MoveSubChar(DWORD base_addr, float* toPos);