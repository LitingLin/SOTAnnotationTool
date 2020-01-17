#include <super_resolution.h>

#include "openvino_pretrained.h"

SuperResolutionAlgorithmInterface::SuperResolutionAlgorithmInterface(const std::wstring& network_model_path,
	const std::wstring& network_weights_path)
{
	_algorithm = new SuperResolutionAlgorithm(network_model_path, network_weights_path);
}

SuperResolutionAlgorithmInterface::~SuperResolutionAlgorithmInterface()
{
	delete _algorithm;
}

size_t SuperResolutionAlgorithmInterface::inputW()
{
	return _algorithm->inputW();
}

size_t SuperResolutionAlgorithmInterface::inputH()
{
	return _algorithm->inputH();
}

size_t SuperResolutionAlgorithmInterface::outputW()
{
	return _algorithm->outputW();
}

size_t SuperResolutionAlgorithmInterface::outputH()
{
	return _algorithm->outputH();
}

void SuperResolutionAlgorithmInterface::setInputBuffer(uint8_t* pointer)
{
	_algorithm->setInputBuffer(pointer);
}

void SuperResolutionAlgorithmInterface::setOutputBuffer(uint8_t* pointer)
{
	_algorithm->setOutputBuffer(pointer);
}

void SuperResolutionAlgorithmInterface::infer()
{
	_algorithm->infer();
}
