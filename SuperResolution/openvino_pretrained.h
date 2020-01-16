#pragma once

#include <super_resolution.h>

#include <string_view>
#include <inference_engine.hpp>

class OpenVINOPretrained : public SuperResolutionAlgorithmInterface
{
public:
	OpenVINOPretrained(const std::wstring& network_model_path, const std::wstring& network_weights_path, bool preferGPU);
	~OpenVINOPretrained() override;
	size_t inputW() override;
	size_t inputH() override;
	size_t outputW() override;
	size_t outputH() override;
	// CHW
	float* getInputBuffer() override;
	void infer() override;
	const float* getOutputBuffer() override;
private:
	InferenceEngine::CNNNetwork _network;
	size_t _inputW;
	size_t _inputH;
	size_t _outputW;
	size_t _outputH;
	InferenceEngine::ExecutableNetwork _executableNetwork;
	InferenceEngine::InferRequest _inferRequest;
	float* _inputPtr;
	float* _outputPtr;
};