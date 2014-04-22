// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"

#include "XInput_XboxOne.h"

bool APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
		case DLL_PROCESS_ATTACH:
			connectController(true);
			break;
		case DLL_THREAD_ATTACH:
			break;
		case DLL_THREAD_DETACH:
			break;
		case DLL_PROCESS_DETACH:
			connectController(false);
			break;
	}

	return TRUE;
}

