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
	try {
		std::wstring imagePath = lpCmdLine;
		Base::File imageFile(imagePath);
		Base::MemoryMappedIO imageMemoryMappedIO(&imageFile);

		SuperResolutionAlgorithmInterface superResolutionAlgorithm(L"D:\\single-image-super-resolution-1032.xml", L"D:\\single-image-super-resolution-1032.bin");
		auto inputH = superResolutionAlgorithm.inputH();
		auto inputW = superResolutionAlgorithm.inputW();
		auto outputH = superResolutionAlgorithm.outputH();
		auto outputW = superResolutionAlgorithm.outputW();
		
		cv::Mat image = cv::imdecode(cv::_InputArray((unsigned char*)imageMemoryMappedIO.get(), imageFile.getSize()), cv::ImreadModes::IMREAD_COLOR);
		if (inputH!=image.rows || inputW!=image.cols)
		{
			cv::resize(image, image, cv::Size(inputW, inputH));
		}
		cv::Mat RGBImage;
		cv::cvtColor(image, RGBImage, cv::COLOR_BGR2RGB);

		// do inference
		superResolutionAlgorithm.setInputBuffer(RGBImage.data);

		cv::Mat outputBuffer(outputH, outputW, CV_8UC3);
		
		superResolutionAlgorithm.setOutputBuffer(outputBuffer.data);
		superResolutionAlgorithm.infer();

		cv::cvtColor(outputBuffer, outputBuffer, cv::COLOR_RGB2BGR);
		
		cv::imshow("test", outputBuffer);
		cv::waitKey();
	}
	catch (std::exception &e)
	{
		Base::Logging::log(e.what());
		throw e;
	}
	return 0;
}
