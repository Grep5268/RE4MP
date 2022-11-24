#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <iostream>
#include <Windows.h>
#include <tlhelp32.h>
using namespace std;

char DllPath[] = "D:\\projects\\cpp\\RE4MP\\Debug\\RE4MP.dll";

int main(int argc, char* argv[]) {
	HWND hwnd = FindWindowA(NULL, "Resident Evil 4"); // HWND (Windows window) by Window Name
	DWORD procID; // A 32-bit unsigned integer, DWORDS are mostly used to store Hexadecimal Addresses
	GetWindowThreadProcessId(hwnd, &procID); // Getting our Process ID, as an ex. like 000027AC
	HANDLE handle = OpenProcess(PROCESS_ALL_ACCESS, FALSE, procID); // Opening the Process with All Access

	// Allocate memory for the dllpath in the target process, length of the path string + null terminator
	LPVOID pDllPath = VirtualAllocEx(handle, 0, strlen(DllPath) + 1, MEM_COMMIT, PAGE_READWRITE);

	// Write the path to the address of the memory we just allocated in the target process
	WriteProcessMemory(handle, pDllPath, (LPVOID)DllPath, strlen(DllPath) + 1, 0);

	// Create a Remote Thread in the target process which calls LoadLibraryA as our dllpath as an argument -> program loads our dll
	HANDLE hLoadThread = CreateRemoteThread(handle, 0, 0,
		(LPTHREAD_START_ROUTINE)GetProcAddress(GetModuleHandleA("Kernel32.dll"), "LoadLibraryA"), pDllPath, 0, 0);

	WaitForSingleObject(hLoadThread, INFINITE); // Wait for the execution of our loader thread to finish

	cout << "Dll path allocated at: " << hex << pDllPath << endl;
	cin.get();

	VirtualFreeEx(handle, pDllPath, strlen(DllPath) + 1, MEM_RELEASE); // Free the memory allocated for our dll path

	return 0;
}