#pragma once

class DrawCurrentTargetNativeHelper;

namespace AnnotationTool {
	namespace NativeInteropServices {
		public ref class DrawCurrentTargetNativeHelper
		{
		public:
			value struct Result
			{
				array<System::Byte>^ image;
				size_t imageWidth; size_t imageHeight;
				
				size_t scalingRatio;
				size_t x; size_t y; size_t width; size_t height;
			};
			DrawCurrentTargetNativeHelper();
			~DrawCurrentTargetNativeHelper();

			void initialize(array<System::Byte>^ image, size_t width, size_t height);

			Result update(size_t windowElementPixelWidth, size_t windowElementPixelHeight, size_t boundingBoxX, size_t boundingBoxY, size_t boundingBoxWidth, size_t boundingBoxHeight, double context);
		private:
			::DrawCurrentTargetNativeHelper* _helper;
		};
	}
}