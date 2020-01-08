#pragma once

#ifdef _WINDLL
#define DLLEXPORTS __declspec(dllexport)
#else
#define DLLEXPORTS __declspec(dllimport)
#endif

extern "C" {
	DLLEXPORTS void *createBACFTracker();
	DLLEXPORTS bool initializeBACFTracker(void *handle, unsigned char *data, int width, int height, double boundingBoxX, double boundingBoxY, double boundingBoxW, double boundingBoxH);
	DLLEXPORTS bool predictBACFTracker(void *handle, unsigned char *image, int width, int height, double *boundingBoxX, double *boundingBoxY, double *boundingBoxW, double *boundingBoxH);
	DLLEXPORTS bool destroyBACFTracker(void *handle);
}