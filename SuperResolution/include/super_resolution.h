#pragma once

#if defined COMPILING_SUPER_RESOLUTION && defined _WINDLL
#define SUPER_RESOLUTION_INTERFACE __declspec(dllexport)
#else
#define SUPER_RESOLUTION_INTERFACE
#endif

#include <string>

class SUPER_RESOLUTION_INTERFACE SuperResolutionAlgorithmInterface
{
public:
	virtual ~SuperResolutionAlgorithmInterface();
	virtual size_t inputW() = 0;
	virtual size_t inputH() = 0;
	virtual size_t outputW() = 0;
	virtual size_t outputH() = 0;
	// CHW
	virtual float* getInputBuffer() = 0;
	virtual void infer() = 0;
	virtual const float* getOutputBuffer() = 0;
};

class SUPER_RESOLUTION_INTERFACE SuperResolutionAlgorithmFactory
{
public:
	SuperResolutionAlgorithmInterface* createOpenVINOPretrainedModel(const std::wstring& network_model_path, const std::wstring& network_weights_path, bool preferGPU);
	void destroy(SuperResolutionAlgorithmInterface* instance);
};
