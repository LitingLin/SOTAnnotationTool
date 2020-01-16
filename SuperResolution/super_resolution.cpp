#include <super_resolution.h>

#include "openvino_pretrained.h"

SuperResolutionAlgorithmInterface::~SuperResolutionAlgorithmInterface() = default;

SuperResolutionAlgorithmInterface* SuperResolutionAlgorithmFactory::createOpenVINOPretrainedModel(
	const std::wstring& network_model_path, const std::wstring& network_weights_path, bool preferGPU)
{
	return new OpenVINOPretrained(network_model_path, network_weights_path, preferGPU);
}

void SuperResolutionAlgorithmFactory::destroy(SuperResolutionAlgorithmInterface* instance)
{
	delete instance;
}
