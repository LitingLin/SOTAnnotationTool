#pragma once

class BACFTracker;

namespace AnnotationTool {
	namespace NativeInteropServices {
		public ref class BACFTracker
		{
		public:
			BACFTracker();
			~BACFTracker();
			void Initialize(array<System::Byte>^ image, int width, int height, int boundingBoxX, int boundingBoxY, int boundingBoxWidth, int boundingBoxHeight);
			System::Tuple<int, int, int, int>^ Predict(array<System::Byte>^ image, int width, int height);
		private:
			::BACFTracker* _tracker;
		};
	}
}