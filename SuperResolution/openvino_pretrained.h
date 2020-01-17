#pragma once

// https://arxiv.org/pdf/1807.06779.pdf

#include <super_resolution.h>

#include <string_view>
#include <tuple>
#include <inference_engine.hpp>
#include <opencv2/core/mat.hpp>

class OpenVINOModelDataProcessor
{
public:
	OpenVINOModelDataProcessor(size_t inputW, size_t inputH, size_t outputW, size_t outputH);
	void setPreprocessInputPointer(uint8_t* data);
	void setPreprocessOutputPointer(float* transformed, float* bicubicTransformed);	
	void preprocess();
	void setPostprocessInputPointer(const float* data);
	void setPostprocessOutputPointer(uint8_t* transformed);
	void postprocess();
private:
	size_t _inputW;
	size_t _inputH;
	size_t _outputW;
	size_t _outputH;
	uint8_t* _preprocessingInput;
	float* _preprocessingTransformed;
	float* _preprocessingBicubicTransformed;
	cv::Mat _buffer;
	const float* _postprocessingInput;
	uint8_t* _postprocessingOutput;
};

class OpenVINOPretrainedModel
{
public:
	OpenVINOPretrainedModel(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	size_t inputW();
	size_t inputH();
	size_t outputW();
	size_t outputH();
	std::tuple<float*, float*> getInputBuffer();
	void infer();
	const float* getOutputBuffer();
private:
	InferenceEngine::CNNNetwork _network;
	size_t _inputW;
	size_t _inputH;
	size_t _outputW;
	size_t _outputH;
	InferenceEngine::ExecutableNetwork _executableNetwork;
	InferenceEngine::InferRequest _inferRequest;
	float* _inputPtr;
	float* _inputBicibicPtr;
	float* _outputPtr;
};

class SuperResolutionAlgorithm
{
public:
	SuperResolutionAlgorithm(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	size_t inputW();
	size_t inputH();
	size_t outputW();
	size_t outputH();
	void setInputBuffer(uint8_t* pointer);
	void setOutputBuffer(uint8_t* pointer);
	void infer();
private:
	OpenVINOPretrainedModel _model;
	OpenVINOModelDataProcessor _dataProcessor;
};
