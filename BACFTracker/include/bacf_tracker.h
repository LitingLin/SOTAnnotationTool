#pragma once

#if defined COMPILING_BACF_TRACKER && defined _WINDLL
#define BACF_TRACKER_INTERFACE __declspec(dllexport)
#else
#define BACF_TRACKER_INTERFACE
#endif

#include <tuple>

class KCF_Tracker;

class BACF_TRACKER_INTERFACE BACFTracker
{
public:
	BACFTracker();
	void initialize(unsigned char* image, int width, int height, int boundingBoxX, int boundingBoxY, int boundingBoxWidth, int boundingBoxHeight);
	std::tuple<int, int, int, int> predict(unsigned char* image, int width, int height);
	~BACFTracker();
private:
	KCF_Tracker* _tracker;
};