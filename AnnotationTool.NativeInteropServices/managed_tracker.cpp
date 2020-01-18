#include "managed_tracker.h"

#include <bacf_tracker.h>

#include "utils.h"

namespace AnnotationTool {
	namespace NativeInteropServices {
		BACFTracker::BACFTracker()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			_tracker = new ::BACFTracker;
			EXCEPTION_SAFE_EXECUTION_END
		}

		BACFTracker::~BACFTracker()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			delete _tracker;
			EXCEPTION_SAFE_EXECUTION_END
		}

		void BACFTracker::Initialize(array<System::Byte>^ image, int width, int height, int boundingBoxX, int boundingBoxY, int boundingBoxWidth, int boundingBoxHeight)
		{
			if (image->LongLength != (long long)width * (long long)height * 3)
				throw gcnew System::Exception("Image Size mismatch");
			EXCEPTION_SAFE_EXECUTION_BEGIN
			pin_ptr<unsigned char> imageData = &image[0];
			_tracker->initialize(imageData, width, height, boundingBoxX, boundingBoxY, boundingBoxWidth, boundingBoxHeight);
			EXCEPTION_SAFE_EXECUTION_END
		}

		System::Tuple<int, int, int, int>^ BACFTracker::Predict(array<System::Byte>^ image, int width, int height)
		{
			if (image->LongLength != (long long)width * (long long)height * 3)
				throw gcnew System::Exception("Image Size mismatch");
			EXCEPTION_SAFE_EXECUTION_BEGIN
			pin_ptr<unsigned char> imageData = &image[0];
			auto [predictedX, predictedY, predictedWidth, predictedHeight] = _tracker->predict(imageData, width, height);
			return gcnew System::Tuple<int, int, int, int>(predictedX, predictedY, predictedWidth, predictedHeight);
			EXCEPTION_SAFE_EXECUTION_END
		}
	}
}