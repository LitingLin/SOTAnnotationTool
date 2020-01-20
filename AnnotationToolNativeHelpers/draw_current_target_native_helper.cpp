#include "draw_current_target_native_helper.h"

#include <cmath>
#include <base/logging.h>

DrawCurrentTargetNativeHelper::DrawCurrentTargetNativeHelper(const std::wstring& network_model_path, const std::wstring& network_weights_path)
	: _superResolution(network_model_path, network_weights_path)
{
}

DrawCurrentTargetNativeHelper::~DrawCurrentTargetNativeHelper() = default;

void DrawCurrentTargetNativeHelper::initialize(uint8_t* image, size_t width, size_t height)
{
	_sourceImage = cv::Mat(cv::Size(width, height), CV_8UC3);
	memcpy(_sourceImage.data, image, width * height * 3);

	_superResolution.setSourceImage(_sourceImage.data, _sourceImage.cols, _sourceImage.rows);
}

std::tuple<uint8_t*,
size_t, size_t,
size_t,
size_t, size_t, size_t, size_t> DrawCurrentTargetNativeHelper::update(size_t windowElementPixelWidth, size_t windowElementPixelHeight, size_t boundingBoxX, size_t boundingBoxY, size_t boundingBoxWidth, size_t boundingBoxHeight, double context)
{
	double cropX, cropY, cropWidth, cropHeight;

	double boundingBoxContextSizeX = std::round((double)boundingBoxWidth * context);
	double boundingBoxContextSizeY = std::round((double)boundingBoxHeight * context);

	double boundingBoxWithContextWidth = (double)boundingBoxWidth + 2. * boundingBoxContextSizeX;
	double boundingBoxWithContextHeight = (double)boundingBoxHeight + 2. * boundingBoxContextSizeY;

	double windowElementXYRatio = (double)windowElementPixelWidth / (double)windowElementPixelHeight;

	cropWidth = boundingBoxWithContextWidth;
	cropHeight = boundingBoxWithContextHeight;
	
	if (double(windowElementPixelWidth) / boundingBoxWithContextWidth > double(windowElementPixelHeight) / boundingBoxWithContextHeight)
	{
		cropWidth = std::round(boundingBoxWithContextHeight * windowElementXYRatio);
		cropHeight = boundingBoxWithContextHeight;

		boundingBoxContextSizeX = std::round((cropWidth - (double)boundingBoxWidth) / 2.);
	}
	else
	{
		cropWidth = boundingBoxWithContextWidth;
		cropHeight = std::round(boundingBoxWithContextWidth / windowElementXYRatio);

		boundingBoxContextSizeY = std::round((cropHeight - (double)boundingBoxHeight) / 2.);
	}
	
	cropX = (double)boundingBoxX - boundingBoxContextSizeX;
	cropY = (double)boundingBoxY - boundingBoxContextSizeY;

	cropX = std::round(cropX);
	cropY = std::round(cropY);
	cropWidth = std::round(cropWidth);
	cropHeight = std::round(cropHeight);

	L_ENSURE_GT(cropWidth, 0);
	L_ENSURE_GT(cropHeight, 0);
	
	size_t quantifiedCropWidth = (size_t)cropWidth;
	size_t quantifiedCropHeight = (size_t)cropHeight;

	size_t quantifiedContextWidth = (size_t)((double)boundingBoxX - cropX);
	size_t quantifiedContextHeight = (size_t)((double)boundingBoxY - cropY);
	
	size_t sourceCropX, sourceCropY, sourceCropWidth, sourceCropHeight;
	size_t destinationCropX, destinationCropY;

	size_t sourceImageWidth = _sourceImage.cols;
	size_t sourceImageHeight = _sourceImage.rows;

	if (cropX < 0)
	{
		sourceCropX = 0;
		destinationCropX = (size_t)(-cropX);
	}
	else
	{
		sourceCropX = cropX;
		destinationCropX = 0;
	}

	if (cropY < 0)
	{
		sourceCropY = 0;
		destinationCropY = (size_t)(-cropY);
	}
	else
	{
		sourceCropY = cropY;
		destinationCropY = 0;
	}
	
	if (cropX + cropWidth > sourceImageWidth)
	{
		sourceCropWidth = sourceImageWidth - sourceCropX;
	}
	else
	{
		sourceCropWidth = cropX + cropWidth - sourceCropX;
	}
	if (cropY + cropHeight > sourceImageHeight)
	{
		sourceCropHeight = sourceImageHeight - sourceCropY;
	}
	else
	{
		sourceCropHeight = cropY + cropHeight - sourceCropY;
	}

	L_ENSURE_LE(destinationCropX + sourceCropWidth, sourceImageWidth);
	L_ENSURE_LE(destinationCropY + sourceCropHeight, sourceImageHeight);
	
	if (quantifiedCropWidth > windowElementPixelWidth || quantifiedCropHeight > windowElementPixelHeight)
	{
		auto superResolutionRatio = _superResolution.scalingRatio();
		size_t superResolutionResultWidth = superResolutionRatio * sourceCropWidth;
		size_t superResolutionResultHeight = superResolutionRatio * sourceCropHeight;

		if (_superResolutionCache.rows != superResolutionResultHeight || _superResolutionCache.cols != superResolutionResultWidth)
		{
			_superResolutionCache = cv::Mat(cv::Size(superResolutionResultWidth, superResolutionResultHeight), CV_8UC3, 0.);
		}

		auto result = _superResolution.process(sourceCropX, sourceCropY, sourceCropWidth, sourceCropHeight);
		result.copyTo(_superResolutionCache.data);

		size_t finalCropX = destinationCropX * superResolutionRatio, finalCropY = destinationCropY * superResolutionRatio;
		size_t finalResultWidth = quantifiedCropWidth * superResolutionRatio, finalResultHeight = quantifiedCropHeight * superResolutionRatio;

		if (_resultCache.rows != finalResultHeight || _resultCache.cols != finalResultWidth)
		{
			_resultCache = cv::Mat(cv::Size(finalResultWidth, finalResultHeight), CV_8UC3, 0.);
		}
		else
		{
			_resultCache.setTo(0.);
		}
		
		_superResolutionCache.copyTo(_resultCache(cv::Rect(finalCropX, finalCropY, superResolutionResultWidth, superResolutionResultHeight)));

		return std::make_tuple(_resultCache.data, finalResultWidth, finalResultHeight, superResolutionRatio,
			quantifiedContextWidth * superResolutionRatio, quantifiedContextHeight * superResolutionRatio, boundingBoxWidth * superResolutionRatio, boundingBoxHeight * superResolutionRatio
			);
	}
	else
	{
		if (_resultCache.cols != (int)quantifiedCropWidth || _resultCache.rows != (int)quantifiedCropHeight)
			_resultCache = cv::Mat(cv::Size((int)quantifiedCropWidth, (int)quantifiedCropHeight), CV_8UC3, 0.);
		else
			_resultCache.setTo(0.);
		_sourceImage(cv::Rect(sourceCropX, sourceCropY, sourceCropWidth, sourceCropHeight)).copyTo(_resultCache(cv::Rect(destinationCropX, destinationCropY, sourceCropWidth, sourceCropHeight)));

		return std::make_tuple(_resultCache.data, quantifiedCropWidth, quantifiedCropHeight, 1, quantifiedContextWidth, quantifiedContextHeight, boundingBoxWidth, boundingBoxHeight);
	}
}
