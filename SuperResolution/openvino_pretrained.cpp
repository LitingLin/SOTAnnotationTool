#include "openvino_pretrained.h"

#include <base/logging.h>
#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

inline void HWCToCHWAndUint8ToFloat(const unsigned char* input, float* output, size_t h, size_t w, size_t c)
{
	size_t channel_size = h * w;
	for (size_t i_h = 0; i_h < h; ++i_h)
	{
		for (size_t i_w = 0; i_w < w; ++i_w)
		{
			for (size_t i_c = 0; i_c < c; ++i_c)
			{
				*(output + channel_size * i_c) = *input;
				++input;
			}
			++output;
		}
	}
}

inline void CHWToHWCAndFloatToUint8(const float* input, unsigned char* output, size_t h, size_t w, size_t c, float scale)
{
	for (size_t i_c = 0; i_c < c; ++i_c)
	{
		unsigned char* outputCurrentChannel = output + i_c;
		for (size_t i_h = 0; i_h < h; ++i_h)
		{
			for (size_t i_w = 0; i_w < w; ++i_w)
			{
				float value = (*input) * scale;
				if (value < 0.f) value = 0.f;
				if (value > 255.f) value = 255.f;
				*outputCurrentChannel = uint8_t(value);
				outputCurrentChannel += c;
				++input;
			}
		}
	}
}

OpenVINOModelDataProcessor::OpenVINOModelDataProcessor(size_t inputW, size_t inputH, size_t outputW, size_t outputH) :
	_inputW(inputW), _inputH(inputH), _outputW(outputW), _outputH(outputH), _buffer(outputH, outputW, CV_8UC3)
{
}

void OpenVINOModelDataProcessor::setPreprocessInputPointer(uint8_t* data)
{
	_preprocessingInput = data;
}

void OpenVINOModelDataProcessor::setPreprocessOutputPointer(float* transformed, float* bicubicTransformed)
{
	_preprocessingTransformed = transformed;
	_preprocessingBicubicTransformed = bicubicTransformed;
}

void OpenVINOModelDataProcessor::preprocess()
{
	HWCToCHWAndUint8ToFloat(_preprocessingInput, _preprocessingTransformed, _inputH, _inputW, 3);

	cv::Mat input(_inputH, _inputW, CV_8UC3, _preprocessingInput);
	cv::resize(input, _buffer, cv::Size(_outputW, _outputH), 0, 0, cv::INTER_CUBIC);

	HWCToCHWAndUint8ToFloat(_buffer.data, _preprocessingBicubicTransformed, _outputH, _outputW, 3);
}

void OpenVINOModelDataProcessor::setPostprocessInputPointer(const float* data)
{
	_postprocessingInput = data;
}

void OpenVINOModelDataProcessor::setPostprocessOutputPointer(uint8_t* transformed)
{
	_postprocessingOutput = transformed;
}

void OpenVINOModelDataProcessor::postprocess()
{
	CHWToHWCAndFloatToUint8(_postprocessingInput, _postprocessingOutput, _outputH, _outputW, 3, 255.f);
}

OpenVINOPretrainedModel::OpenVINOPretrainedModel(const std::wstring& network_model_path, const std::wstring& network_weights_path)
{
	InferenceEngine::Core core;

	InferenceEngine::CNNNetReader networkReader;
	networkReader.ReadNetwork(network_model_path);
	networkReader.ReadWeights(network_weights_path);

	_network = networkReader.getNetwork();

	_network.setBatchSize(1);
	
	/** Taking information about all topology inputs **/
	InferenceEngine::InputsDataMap inputInfo = _network.getInputsInfo();

	L_ENSURE_EQ(inputInfo.size(), 2);
	auto& inputInfoItem = inputInfo.at("0");
	auto& inputTensorDesc = inputInfoItem->getTensorDesc();
	L_ENSURE_EQ(inputTensorDesc.getDims().size(), 4);
	size_t w = inputTensorDesc.getDims()[3];
	size_t h = inputTensorDesc.getDims()[2];
	size_t c = inputTensorDesc.getDims()[1];
	L_ENSURE_EQ(inputTensorDesc.getDims()[0], 1);
	L_ENSURE_EQ(c, 3);
	
	auto& bicubicInputInfoItem = inputInfo.at("1");
	auto& bicubicInputTensorDesc = bicubicInputInfoItem->getTensorDesc();
	L_ENSURE_EQ(bicubicInputTensorDesc.getDims().size(), 4);
	size_t bicubicInputW = bicubicInputTensorDesc.getDims()[3];
	size_t bicubicInputH = bicubicInputTensorDesc.getDims()[2];
	size_t bicubicInputC = bicubicInputTensorDesc.getDims()[1];
	L_ENSURE_EQ(bicubicInputTensorDesc.getDims()[0], 1);
	L_ENSURE_EQ(bicubicInputC, 3);
	
	_inputW = w;
	_inputH = h;


	auto outputsInfo = _network.getOutputsInfo();
	L_ENSURE_EQ(outputsInfo.size(), 1);
	auto& outputName = outputsInfo.begin()->first;
	L_ENSURE(outputsInfo.begin()->second);
	outputsInfo.begin()->second->setPrecision(InferenceEngine::Precision::FP32);

	auto availableDevices = core.GetAvailableDevices();
	std::string cpuDeviceName;
	for (auto& device : availableDevices)
	{
		if (device.size() < 3)
			continue;
		if (device.compare(0, 3, "CPU") == 0) {
			cpuDeviceName = device;
			break;
		}
	}

	L_ENSURE(!cpuDeviceName.empty());

	_executableNetwork = core.LoadNetwork(_network, cpuDeviceName);


	// --------------------------- 5. Create infer request -------------------------------------------------

	_inferRequest = _executableNetwork.CreateInferRequest();

	// -----------------------------------------------------------------------------------------------------

	// --------------------------- 6. Prepare input --------------------------------------------------------
	InferenceEngine::Blob::Ptr lrInputBlob = _inferRequest.GetBlob("0");
	_inputPtr = lrInputBlob->buffer().as<float*>();
	L_ENSURE(_inputPtr);
	InferenceEngine::Blob::Ptr lrBicubicInputBlob = _inferRequest.GetBlob("1");
	_inputBicibicPtr = lrBicubicInputBlob->buffer().as<float*>();
	L_ENSURE(_inputBicibicPtr);

	// -----------------------------------------------------------------------------------------------------

	// --------------------------- 8. Process output -------------------------------------------------------
	const InferenceEngine::Blob::Ptr outputBlob = _inferRequest.GetBlob(outputName);
	_outputPtr = outputBlob->buffer().as<float*>();
	L_ENSURE(_outputPtr);

	size_t numOfImages = outputBlob->getTensorDesc().getDims()[0];
	size_t numOfChannels = outputBlob->getTensorDesc().getDims()[1];
	size_t outputH = outputBlob->getTensorDesc().getDims()[2];
	size_t outputW = outputBlob->getTensorDesc().getDims()[3];

	L_ENSURE_EQ(bicubicInputW, outputW);
	L_ENSURE_EQ(bicubicInputH, outputH);

	L_ENSURE_EQ(numOfImages, 1);
	L_ENSURE_EQ(numOfChannels, 3);
	_outputH = outputH;
	_outputW = outputW;
}

std::tuple<float*, float*> OpenVINOPretrainedModel::getInputBuffer()
{
	return std::make_tuple(_inputPtr, _inputBicibicPtr);
}

void SuperResolutionAlgorithm::infer()
{
	_dataProcessor.preprocess();
	_model.infer();
	_dataProcessor.postprocess();
}

size_t OpenVINOPretrainedModel::inputW()
{
	return _inputW;
}

size_t OpenVINOPretrainedModel::inputH()
{
	return _inputH;
}

size_t OpenVINOPretrainedModel::outputW()
{
	return _outputW;
}

size_t OpenVINOPretrainedModel::outputH()
{
	return _outputH;
}

void OpenVINOPretrainedModel::infer()
{
	_inferRequest.Infer();
}

const float* OpenVINOPretrainedModel::getOutputBuffer()
{
	return _outputPtr;
}

SuperResolutionAlgorithm::SuperResolutionAlgorithm(const std::wstring& network_model_path,
	const std::wstring& network_weights_path)
	: _model(network_model_path, network_weights_path), _dataProcessor(_model.inputW(), _model.inputH(), _model.outputW(), _model.outputH())
{
	auto [input1, input2] = _model.getInputBuffer();
	_dataProcessor.setPreprocessOutputPointer(input1, input2);
	
	_dataProcessor.setPostprocessInputPointer(_model.getOutputBuffer());
}

size_t SuperResolutionAlgorithm::inputW()
{
	return _model.inputW();
}

size_t SuperResolutionAlgorithm::inputH()
{
	return _model.inputH();
}

size_t SuperResolutionAlgorithm::outputW()
{
	return _model.outputW();
}

size_t SuperResolutionAlgorithm::outputH()
{
	return _model.outputH();
}

void SuperResolutionAlgorithm::setInputBuffer(uint8_t* pointer)
{
	_dataProcessor.setPreprocessInputPointer(pointer);
}

void SuperResolutionAlgorithm::setOutputBuffer(uint8_t* pointer)
{
	_dataProcessor.setPostprocessOutputPointer(pointer);
}
