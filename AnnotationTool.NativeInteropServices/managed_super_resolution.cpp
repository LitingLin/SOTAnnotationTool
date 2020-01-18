#include "managed_super_resolution.h"

#include <region_based_super_resolution.h>

#include <base/file.h>

#include "utils.h"

namespace AnnotationTool {
	namespace NativeInteropServices {
		SuperResolution::SuperResolution()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			std::wstring moduleParentPath = Base::getParentPath(Base::getApplicationPath());

			_algorithm = new RegionBasedSuperResolution(Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.xml"), Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.bin"));
			EXCEPTION_SAFE_EXECUTION_END
		}

		SuperResolution::~SuperResolution()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			delete _algorithm;
			EXCEPTION_SAFE_EXECUTION_END
		}

		void SuperResolution::SetSourceImage(array<System::Byte>^ image, int width, int height)
		{
			if (image->LongLength != (long long)width * (long long)height * 3)
				throw gcnew System::Exception("Image Size mismatch");

			EXCEPTION_SAFE_EXECUTION_BEGIN
			pin_ptr<unsigned char> imageData = &image[0];
			_algorithm->setSourceImage(imageData, width, height);
			EXCEPTION_SAFE_EXECUTION_END
		}

		System::Tuple<array<System::Byte>^, int, int>^ SuperResolution::Process(int regionX, int regionY, int regionWidth, int regionHeight)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN			
			auto result = _algorithm->process(regionX, regionY, regionWidth, regionHeight);
			
			auto [scaledWidth, scaledHeight] = result.getSize();
			array<System::Byte>^ scaledImage = gcnew array<System::Byte>(scaledWidth * scaledHeight * 3);

			pin_ptr<unsigned char> scaledImageData = &scaledImage[0];
			result.copyTo(scaledImageData);

			return gcnew System::Tuple<array<System::Byte>^, int, int>(scaledImage, scaledWidth, scaledHeight);
			EXCEPTION_SAFE_EXECUTION_END
		}
	}
}