#pragma once

#if defined COMPILING_SUPER_RESOLUTION && defined _WINDLL
#define SUPER_RESOLUTION_INTERFACE __declspec(dllexport)
#else
#define SUPER_RESOLUTION_INTERFACE
#endif

#include <string>

class SuperResolutionAlgorithm;

class SUPER_RESOLUTION_INTERFACE SuperResolutionAlgorithmInterface
{
public:
	SuperResolutionAlgorithmInterface(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	~SuperResolutionAlgorithmInterface();
	size_t inputW();
	size_t inputH();
	size_t outputW();
	size_t outputH();
	void setInputBuffer(uint8_t* pointer);
	void setOutputBuffer(uint8_t* pointer);
	void infer();
private:
	SuperResolutionAlgorithm* _algorithm;
};
