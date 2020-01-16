#pragma once

#include <string_view>
#include <inference_engine.hpp>

class OpenVINOPretrained
{
public:
	OpenVINOPretrained(const std::wstring& network_model_path, const std::wstring& network_weights_path, bool preferGPU);
	size_t inputW();
	size_t inputH();
private:
	InferenceEngine::CNNNetwork _network;
	size_t _w;
	size_t _h;
	
};