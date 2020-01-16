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
	
    _network.setBatchSize(1);
    if (preferGPU)
        _network.setTargetDevice(InferenceEngine::TargetDevice::eGPU);
    else
        _network.setTargetDevice(InferenceEngine::TargetDevice::eCPU);

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
	
    InferenceEngine::ExecutableNetwork executableNetwork;

    if (preferGPU && !gpuDeviceName.empty())
        executableNetwork = core.LoadNetwork(_network, gpuDeviceName);
    else
        executableNetwork = core.LoadNetwork(_network, cpuDeviceName);

	
    // --------------------------- 5. Create infer request -------------------------------------------------
    
    InferenceEngine::InferRequest inferRequest = executableNetwork.CreateInferRequest();
	
    // -----------------------------------------------------------------------------------------------------

    // --------------------------- 6. Prepare input --------------------------------------------------------
    InferenceEngine::Blob::Ptr lrInputBlob = inferRequest.GetBlob("0");
	
    for (size_t i = 0; i < inputImages.size(); ++i) {
        cv::Mat img = inputImages[i];
        matU8ToBlob<float_t>(img, lrInputBlob, i);

        bool twoInputs = inputInfo.size() == 2;
        if (twoInputs) {
            const std::string bicInputBlobName = "1";
            Blob::Ptr bicInputBlob = inferRequest.GetBlob(bicInputBlobName);

            int w = bicInputBlob->getTensorDesc().getDims()[3];
            int h = bicInputBlob->getTensorDesc().getDims()[2];

            cv::Mat resized;
            cv::resize(img, resized, cv::Size(w, h), 0, 0, cv::INTER_CUBIC);

            matU8ToBlob<float_t>(resized, bicInputBlob, i);
        }
    }
    // -----------------------------------------------------------------------------------------------------

    // --------------------------- 7. Do inference ---------------------------------------------------------
    std::cout << "To close the application, press 'CTRL+C' here";
    if (FLAGS_show) {
        std::cout << " or switch to the output window and press any key";
    }
    std::cout << std::endl;

    slog::info << "Start inference" << slog::endl;
    inferRequest.Infer();
    // -----------------------------------------------------------------------------------------------------

    // --------------------------- 8. Process output -------------------------------------------------------
    const Blob::Ptr outputBlob = inferRequest.GetBlob(firstOutputName);
    const auto outputData = outputBlob->buffer().as<PrecisionTrait<Precision::FP32>::value_type*>();

    size_t numOfImages = outputBlob->getTensorDesc().getDims()[0];
    size_t numOfChannels = outputBlob->getTensorDesc().getDims()[1];
    size_t h = outputBlob->getTensorDesc().getDims()[2];
    size_t w = outputBlob->getTensorDesc().getDims()[3];
    size_t nunOfPixels = w * h;

    slog::info << "Output size [N,C,H,W]: " << numOfImages << ", " << numOfChannels << ", " << h << ", " << w << slog::endl;

    for (size_t i = 0; i < numOfImages; ++i) {
        std::vector<cv::Mat> imgPlanes;
        if (numOfChannels == 3) {
            imgPlanes = std::vector<cv::Mat>{
                  cv::Mat(h, w, CV_32FC1, &(outputData[i * nunOfPixels * numOfChannels])),
                  cv::Mat(h, w, CV_32FC1, &(outputData[i * nunOfPixels * numOfChannels + nunOfPixels])),
                  cv::Mat(h, w, CV_32FC1, &(outputData[i * nunOfPixels * numOfChannels + nunOfPixels * 2])) };
        }
        else {
            imgPlanes = std::vector<cv::Mat>{ cv::Mat(h, w, CV_32FC1, &(outputData[i * nunOfPixels * numOfChannels])) };

            // Post-processing for text-image-super-resolution models
            cv::threshold(imgPlanes[0], imgPlanes[0], 0.5f, 1.0f, cv::THRESH_BINARY);
        };

        for (auto& img : imgPlanes)
            img.convertTo(img, CV_8UC1, 255);

        cv::Mat resultImg;
        cv::merge(imgPlanes, resultImg);

        if (FLAGS_show) {
            cv::imshow("result", resultImg);
            cv::waitKey();
        }

        std::string outImgName = std::string("sr_" + std::to_string(i + 1) + ".png");
        cv::imwrite(outImgName, resultImg);
    }
    // -----------------------------------------------------------------------------------------------------
}
catch (const std::exception & error) {
    slog::err << error.what() << slog::endl;
    return 1;
}
catch (...) {
    slog::err << "Unknown/internal exception happened" << slog::endl;
    return 1;
}

slog::info << "Execution successful" << slog::endl;
slog::info << slog::endl << "This demo is an API example, for any performance measurements "
"please use the dedicated benchmark_app tool from the openVINO toolkit" << slog::endl;
return 0;
}
