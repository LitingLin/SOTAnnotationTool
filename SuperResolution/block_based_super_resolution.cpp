#include "block_based_super_resolution.h"

#include <base/logging.h>

BlockBasedSuperResolution::BlockBasedSuperResolution(const std::wstring& network_model_path,
	const std::wstring& network_weights_path)
	: _algorithm(network_model_path, network_weights_path), _inputBlockBuffer(_algorithm.inputH(), _algorithm.inputW(), CV_8UC3)
{
	_algorithm.setInputBuffer(_inputBlockBuffer.data);
}

void BlockBasedSuperResolution::setSourceImage(uint8_t* image, size_t width, size_t height)
{
	size_t blockW = _algorithm.inputW();
	size_t blockH = _algorithm.inputH();
	
	size_t scaledBlockW = _algorithm.outputW();
	size_t scaledBlockH = _algorithm.outputH();

	size_t ratio = scaledBlockW / blockW;
	L_ENSURE_EQ(ratio, scaledBlockH / blockH);

	size_t inputBufferWidth, inputBufferHeight;
	size_t outputBufferWidth, outputBufferHeight;
	size_t actualInputWidth, actualInputHeight;
	size_t actualOutputWidth, actualOutputHeight;

	
}

void BlockBasedSuperResolution::processInRegion(size_t x, size_t y, size_t width, size_t height)
{
	
}
