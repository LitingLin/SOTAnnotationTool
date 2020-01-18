#include "region_based_super_resolution.h"

#include <base/logging.h>

RegionBasedSuperResolution::RegionBasedSuperResolution(const std::wstring& network_model_path,
                                                     const std::wstring& network_weights_path)
	: _algorithm(network_model_path, network_weights_path), _inputBlockBuffer(_algorithm.inputH(), _algorithm.inputW(), CV_8UC3), _outputBlockBuffer(_algorithm.outputH(), _algorithm.outputW(), CV_8UC3)
{
	_algorithm.setInputBuffer(_inputBlockBuffer.data);
	_algorithm.setOutputBuffer(_outputBlockBuffer.data);
}

void RegionBasedSuperResolution::setSourceImage(uint8_t* image, size_t width, size_t height)
{
	size_t blockW = _algorithm.inputW();
	size_t blockH = _algorithm.inputH();
	
	size_t scaledBlockW = _algorithm.outputW();
	size_t scaledBlockH = _algorithm.outputH();

	size_t ratio = scaledBlockW / blockW;
	L_ENSURE_EQ(ratio, scaledBlockH / blockH);

	size_t maskWidth = (width + blockW - 1) / blockW, maskHeight = (height + blockH - 1) / blockH;
	size_t inputBufferWidth = maskWidth * blockW, inputBufferHeight = maskHeight * blockH;
	size_t outputBufferWidth = inputBufferWidth * ratio, outputBufferHeight = inputBufferHeight * ratio;

	_inputBuffer = cv::Mat(inputBufferHeight, inputBufferWidth, CV_8UC3, 0.);
	_outputBuffer = cv::Mat(outputBufferHeight, outputBufferWidth, CV_8UC3, 0.);
	_mask = cv::Mat(maskHeight, maskWidth, CV_8U, 0.);

	cv::Mat sourceImage(height, width, CV_8UC3, image);
	sourceImage.copyTo(_inputBuffer(cv::Rect(0, 0, width, height)));

	_actualInputWidth = width;
	_actualInputHeight = height;
}

RegionBasedSuperResolution::Result RegionBasedSuperResolution::process(size_t x, size_t y, size_t width, size_t height)
{
	return Result(this, x, y, width, height);
}

std::tuple<size_t, size_t> RegionBasedSuperResolution::Result::getSize()
{
	auto ratio = _parent->scalingRatio();
	return std::make_tuple(_width * ratio, _height * ratio);
}

void RegionBasedSuperResolution::Result::copyTo(uint8_t* buffer)
{
	auto ratio = _parent->scalingRatio();

	size_t outputX = _x * ratio, outputY = _y * ratio, outputWidth = _width * ratio, outputHeight = _height * ratio;
	
	cv::Mat outputBuffer(outputHeight, outputWidth, CV_8UC3, buffer);
	_parent->_outputBuffer(cv::Rect(outputX, outputY, outputWidth, outputHeight)).copyTo(outputBuffer);
}

RegionBasedSuperResolution::Result::Result(RegionBasedSuperResolution* parent,
                                          size_t x, size_t y, size_t width, size_t height)
	: _parent(parent), _x(x), _y(y), _width(width), _height(height)
{
	L_CHECK_LT(x, parent->_actualInputWidth);
	L_CHECK_LT(y, parent->_actualInputHeight);
	size_t xEnd = x + width, yEnd = y + height;
	L_CHECK_LE(xEnd, parent->_actualInputWidth);
	L_CHECK_LE(yEnd, parent->_actualInputHeight);

	size_t blockW = parent->_algorithm.inputW();
	size_t blockH = parent->_algorithm.inputH();

	size_t scaledBlockW = parent->_algorithm.outputW();
	size_t scaledBlockH = parent->_algorithm.outputH();
	
	size_t xBeginInMask = x / blockW, yBeginInMask = y / blockH, xEndInMask = (xEnd + blockW - 1) / blockW, yEndInMask = (yEnd + blockH - 1) / blockH;
	
	for (size_t xInMask = xBeginInMask; xInMask < xEndInMask; ++xInMask)
	{
		for (size_t yInMask = yBeginInMask; yInMask < yEndInMask; ++yInMask) {
			auto& mask = parent->_mask.at<unsigned char>(yInMask, xInMask);
			if (!mask)
			{
				parent->_inputBuffer(cv::Rect(xInMask * blockW, yInMask * blockH, blockW, blockH)).copyTo(parent->_inputBlockBuffer);				
				parent->_algorithm.infer();
				parent->_outputBlockBuffer.copyTo(parent->_outputBuffer(cv::Rect(xInMask * scaledBlockW, yInMask * scaledBlockH, scaledBlockW, scaledBlockH)));

				mask = 1;
			}
		}
	}
}

size_t RegionBasedSuperResolution::scalingRatio()
{
	return _algorithm.outputW() / _algorithm.inputW();
}
