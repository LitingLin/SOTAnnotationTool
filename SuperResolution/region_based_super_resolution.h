#pragma once

#include "openvino_pretrained.h"

class SUPER_RESOLUTION_INTERFACE RegionBasedSuperResolution
{
public:
	class SUPER_RESOLUTION_INTERFACE Result
	{
	public:
		Result(RegionBasedSuperResolution* parent, size_t x, size_t y, size_t width, size_t height);
		std::tuple<size_t, size_t> getSize();
		void copyTo(uint8_t* buffer);		
	private:
		RegionBasedSuperResolution* _parent;
		size_t _x, _y, _width, _height;		
	};
	RegionBasedSuperResolution(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	void setSourceImage(uint8_t* image, size_t width, size_t height);
	Result process(size_t x, size_t y, size_t width, size_t height);
	size_t scalingRatio();
private:
	SuperResolutionAlgorithm _algorithm;
	
	cv::Mat _inputBuffer;		
	cv::Mat _outputBuffer;
	cv::Mat _inputBlockBuffer;
	cv::Mat _outputBlockBuffer;
	cv::Mat _mask;
	
	size_t _actualInputWidth, _actualInputHeight;
};
