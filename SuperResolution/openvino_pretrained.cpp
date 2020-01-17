#include "openvino_pretrained.h"

#include <base/logging.h>

OpenVINOPretrained::OpenVINOPretrained(const std::wstring& network_model_path, const std::wstring& network_weights_path, bool preferGPU)
{
    InferenceEngine::Core core;
	
	InferenceEngine::CNNNetReader networkReader;
	networkReader.ReadNetwork(network_model_path);
	networkReader.ReadWeights(network_weights_path);

	_network = networkReader.getNetwork();

	/** Taking information about all topology inputs **/
	InferenceEngine::InputsDataMap inputInfo = _network.getInputsInfo();

    L_ENSURE_EQ(inputInfo.size(), 1);
    auto& inputInfoItem = inputInfo.at("0");
    auto& inputTensorDesc = inputInfoItem->getTensorDesc();
    L_ENSURE_EQ(inputTensorDesc.getDims().size(), 4);
    size_t w = inputTensorDesc.getDims()[3];
    size_t h = inputTensorDesc.getDims()[2];
    size_t c = inputTensorDesc.getDims()[1];
    L_ENSURE_EQ(c, 3);
    _inputW = w;
    _inputH = h;
	
    _network.setBatchSize(1);

    auto outputsInfo = _network.getOutputsInfo();
    L_ENSURE_EQ(outputsInfo.size(), 1);
    auto& outputName = outputsInfo.begin()->first;
    L_ENSURE(outputsInfo.begin()->second);
    outputsInfo.begin()->second->setPrecision(InferenceEngine::Precision::FP32);

    auto availableDevices = core.GetAvailableDevices();
    std::string cpuDeviceName, gpuDeviceName;
    for (auto& device : availableDevices)
    {
    	if (device.size() < 3)
            continue;
        if (device.compare(0, 3, "CPU") == 0)
            cpuDeviceName = device;
        if (device.compare(0, 3, "GPU") == 0)
            gpuDeviceName = device;

    	if (!cpuDeviceName.empty() && !gpuDeviceName.empty())
            break;
    }

    L_ENSURE(!(cpuDeviceName.empty() && gpuDeviceName.empty()));
	
    if (preferGPU && !gpuDeviceName.empty())
        _executableNetwork = core.LoadNetwork(_network, gpuDeviceName);
    else
        _executableNetwork = core.LoadNetwork(_network, cpuDeviceName);

	
    // --------------------------- 5. Create infer request -------------------------------------------------
    
    _inferRequest = _executableNetwork.CreateInferRequest();
	
    // -----------------------------------------------------------------------------------------------------

    // --------------------------- 6. Prepare input --------------------------------------------------------
    InferenceEngine::Blob::Ptr lrInputBlob = _inferRequest.GetBlob("0");
    _inputPtr = lrInputBlob->buffer().as<float*>();
	
    // -----------------------------------------------------------------------------------------------------

    // --------------------------- 8. Process output -------------------------------------------------------
    const InferenceEngine::Blob::Ptr outputBlob = _inferRequest.GetBlob(outputName);
    _outputPtr = outputBlob->buffer().as<float*>();

    size_t numOfImages = outputBlob->getTensorDesc().getDims()[0];
    size_t numOfChannels = outputBlob->getTensorDesc().getDims()[1];
    size_t outputH = outputBlob->getTensorDesc().getDims()[2];
    size_t outputW = outputBlob->getTensorDesc().getDims()[3];

    L_ENSURE_EQ(numOfImages, 1);
    L_ENSURE_EQ(numOfChannels, 3);
    _outputH = outputH;
    _outputW = outputW;	
}


float* OpenVINOPretrained::getInputBuffer()
{
    return _inputPtr;
}

size_t OpenVINOPretrained::inputW()
{
    return _inputW;
}

size_t OpenVINOPretrained::inputH()
{
    return _inputH;
}

size_t OpenVINOPretrained::outputW()
{
    return _outputW;
}

size_t OpenVINOPretrained::outputH()
{
    return _outputH;
}

void OpenVINOPretrained::infer()
{
    _inferRequest.Infer();	
}

const float* OpenVINOPretrained::getOutputBuffer()
{
    return _outputPtr;
}

OpenVINOPretrained::~OpenVINOPretrained() = default;
