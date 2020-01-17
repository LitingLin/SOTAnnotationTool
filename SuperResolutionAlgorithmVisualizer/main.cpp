#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>

#include <base/logging/interface.h>
#include <base/logging/sinks/stdout.h>
#include <base/logging/sinks/win32_debugger.h>
#include <base/file.h>
#include <base/memory_mapped_io.h>

#include <opencv2/core.hpp>
#include <opencv2/highgui.hpp>
#include <opencv2/imgproc.hpp>

#include <super_resolution.h>

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

int __stdcall wWinMain(HINSTANCE hInstance,
	HINSTANCE hPrevInstance,
	LPWSTR lpCmdLine,
	int nShowCmd
)
{
	Base::Logging::addSink(new Base::Logging::StdOutSink);
	Base::Logging::addSink(new Base::Logging::Win32DebuggerSink);
	try {
		std::wstring imagePath = lpCmdLine;
		Base::File imageFile(imagePath);
		Base::MemoryMappedIO imageMemoryMappedIO(&imageFile);

		cv::Mat image = cv::imdecode(cv::_InputArray((unsigned char*)imageMemoryMappedIO.get(), imageFile.getSize()), cv::ImreadModes::IMREAD_COLOR);
		cv::Mat RGBImage;

		SuperResolutionAlgorithmFactory factory;
		std::shared_ptr<SuperResolutionAlgorithmInterface> superResolutionAlgorithm(factory.createOpenVINOPretrainedModel(L"D:\\single-image-super-resolution-1032.xml", L"D:\\single-image-super-resolution-1032.bin", true), [&factory](auto p) {factory.destroy(p); });
		auto inputH = superResolutionAlgorithm->inputH();
		auto inputW = superResolutionAlgorithm->inputW();
		cv::resize(image, RGBImage, cv::Size(inputW, inputH));
		// do inference
		float* inputPtr = superResolutionAlgorithm->getInputBuffer();
		HWCToCHWAndUint8ToFloat(RGBImage.data, inputPtr, inputH, inputW, 3);
		superResolutionAlgorithm->infer();
		const float* outputPtr = superResolutionAlgorithm->getOutputBuffer();

		cv::imshow("test", image);
		cv::waitKey();
	}
	catch (std::exception &e)
	{
		Base::Logging::log(e.what());
		throw e;
	}
	return 0;
}
