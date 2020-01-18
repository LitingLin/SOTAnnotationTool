#pragma once

#include <region_based_super_resolution.h>

class RegionBasedSuperResolution;

namespace AnnotationTool {
	namespace NativeInteropServices {
		
		public ref class SuperResolution
		{
		public:
			SuperResolution();
			~SuperResolution();
			void SetSourceImage(array<System::Byte>^ image, int width, int height);
			System::Tuple<array<System::Byte>^, int, int>^ Process(int regionX, int regionY, int regionWidth, int regionHeight);
		private:
			::RegionBasedSuperResolution* _algorithm;
		};
	}
}