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

int __stdcall wWinMain(HINSTANCE hInstance,
	HINSTANCE hPrevInstance,
	LPWSTR lpCmdLine,
	int nShowCmd
)
{
	Base::Logging::addSink(new Base::Logging::StdOutSink);
	Base::Logging::addSink(new Base::Logging::Win32DebuggerSink);

	try
	{
		std::wstring imagePath = lpCmdLine;
		Base::File imageFile(imagePath);
		Base::MemoryMappedIO imageMemoryMappedIO(&imageFile);

		cv::Mat image = cv::imdecode(cv::_InputArray(imageMemoryMappedIO.get(), imageFile.getSize()), cv::ImreadModes::IMREAD_COLOR);
		cv::Mat RGBImage;
		cv::cvtColor(image, RGBImage, cv::COLOR_BGR2RGB);

		SuperResolutionAlgorithmFactory factory;
		auto superResolutionAlgorithm = factory.createOpenVINOPretrainedModel(L"", L"", true);
		auto inputH = superResolutionAlgorithm->inputH();
		auto inputW = superResolutionAlgorithm->inputW();
		cv::resize(image, RGBImage, cv::Size(inputW, inputH));
		// do inference


		cv::imshow("test", image);
		cv::waitKey();
	}
	catch (...)
	{
		return -1;
	}
	return 0;
}
