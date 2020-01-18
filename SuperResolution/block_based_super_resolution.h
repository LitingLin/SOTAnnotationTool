#pragma once

#include "openvino_pretrained.h"

class BlockBasedSuperResolution
{
	struct BoundingBox
	{
		size_t x;
		size_t y;
		size_t width;
		size_t height;
	};
public:
	BlockBasedSuperResolution(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	void setSourceImage(uint8_t* image, size_t width, size_t height);
	void processInRegion(size_t x, size_t y, size_t width, size_t height);
	std::tuple<size_t, size_t> getResultSize();
	void copyResultTo(uint8_t* buffer);
private:
	SuperResolutionAlgorithm _algorithm;
	cv::Mat _inputBuffer;
	
	uint8_t* image;
	size_t _inputWidth;
	size_t _inputHeight;
	
	cv::Mat _outputBuffer;
	cv::Mat _inputBlockBuffer;
	cv::Mat _mask;
};
