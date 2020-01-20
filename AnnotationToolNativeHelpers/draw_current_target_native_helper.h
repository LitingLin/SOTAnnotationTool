#pragma once

#if defined COMPILING_NATIVE_HELPERS && defined _WINDLL
#define NATIVE_HELPERS_INTERFACE __declspec(dllexport)
#else
#define NATIVE_HELPERS_INTERFACE
#endif

#include <stdint.h>
#include <tuple>
#include <opencv2/core/mat.hpp>
#include <region_based_super_resolution.h>

class NATIVE_HELPERS_INTERFACE DrawCurrentTargetNativeHelper
{
public:
	DrawCurrentTargetNativeHelper(const std::wstring& network_model_path, const std::wstring& network_weights_path);
	~DrawCurrentTargetNativeHelper();
	void initialize(uint8_t* image, size_t width, size_t height);
	std::tuple<uint8_t*,
		size_t, size_t,
		size_t,
		size_t, size_t, size_t, size_t> update(size_t windowElementPixelWidth, size_t windowElementPixelHeight, size_t boundingBoxX, size_t boundingBoxY, size_t boundingBoxWidth, size_t boundingBoxHeight, double context);
private:
	cv::Mat _sourceImage;

	cv::Mat _resultCache;
	
	cv::Mat _superResolutionCache;
	RegionBasedSuperResolution _superResolution;
};
