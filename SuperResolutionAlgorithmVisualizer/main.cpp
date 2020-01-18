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

#include <region_based_super_resolution.h>


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

		std::wstring moduleParentPath = Base::getParentPath(Base::getApplicationPath());
		
		RegionBasedSuperResolution superResolutionAlgorithm(Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.xml"), Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.bin"));
		
		cv::Mat image = cv::imdecode(cv::_InputArray((unsigned char*)imageMemoryMappedIO.get(), imageFile.getSize()), cv::ImreadModes::IMREAD_COLOR);
		cv::Mat RGBImage;
		cv::cvtColor(image, RGBImage, cv::COLOR_BGR2RGB);

		superResolutionAlgorithm.setSourceImage(RGBImage.data, RGBImage.cols, RGBImage.rows);

		auto scale = superResolutionAlgorithm.scalingRatio();
		cv::Mat outputBuffer(RGBImage.rows * scale, RGBImage.cols * scale, CV_8UC3);
		
		auto result = superResolutionAlgorithm.process(0, 0, RGBImage.cols, RGBImage.rows);
		result.copyTo(outputBuffer.data);

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
