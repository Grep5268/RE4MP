#pragma once
#include <Windows.h>

void HookFunctions(DWORD base_address);
void CodeInjection(DWORD base_addr);
int* SubCharPointer(DWORD base_addr);
void MoveSubChar(DWORD base_addr, float* toPos);