#include "managed_native_helpers.h"

#include <draw_current_target_native_helper.h>
#include <base/file.h>

#include "utils.h"

namespace AnnotationTool {
	namespace NativeInteropServices {
		DrawCurrentTargetNativeHelper::DrawCurrentTargetNativeHelper()
		{
			std::wstring moduleParentPath = Base::getParentPath(Base::getApplicationPath());

			_helper = new ::DrawCurrentTargetNativeHelper(Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.xml"), Base::appendPath(moduleParentPath, L"single-image-super-resolution-1032.bin"));
		}

		DrawCurrentTargetNativeHelper::~DrawCurrentTargetNativeHelper()
		{
			delete _helper;
		}

		void DrawCurrentTargetNativeHelper::initialize(array<System::Byte>^ image, size_t width, size_t height)
		{
			if (image->LongLength != (long long)width * (long long)height * 3)
				throw gcnew System::Exception("Image Size mismatch");

			EXCEPTION_SAFE_EXECUTION_BEGIN
			pin_ptr<unsigned char> imageData = &image[0];
			_helper->initialize(imageData, width, height);
			EXCEPTION_SAFE_EXECUTION_END
		}

		DrawCurrentTargetNativeHelper::Result
			DrawCurrentTargetNativeHelper::update(size_t windowElementPixelWidth, size_t windowElementPixelHeight,
				size_t boundingBoxX, size_t boundingBoxY, size_t boundingBoxWidth, size_t boundingBoxHeight, double context)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN

				auto [resultImagePtr, resultImageWidth, resultImageHeight, superResolutionRatio, resultBoundingBoxX, resultBoundingBoxY, resultBoundingBoxWidth, resultBoundingBoxHeight] = _helper->update(windowElementPixelWidth, windowElementPixelHeight, boundingBoxX, boundingBoxY, boundingBoxWidth, boundingBoxHeight, context);

			array<System::Byte>^ resultImage = gcnew array<System::Byte>(resultImageWidth * resultImageHeight * 3);
			{
				pin_ptr<unsigned char> resultImageData = &resultImage[0];
				memcpy(resultImageData, resultImagePtr, resultImageWidth * resultImageHeight * 3);
			}
			DrawCurrentTargetNativeHelper::Result result;
			result.image = resultImage;
			result.imageWidth = resultImageWidth;
			result.imageHeight = resultImageHeight;
			result.scalingRatio = superResolutionRatio;
			result.x = resultBoundingBoxX;
			result.y = resultBoundingBoxY;
			result.width = resultBoundingBoxWidth;
			result.height = resultBoundingBoxHeight;
			return result;

			EXCEPTION_SAFE_EXECUTION_END
		}
	}
}