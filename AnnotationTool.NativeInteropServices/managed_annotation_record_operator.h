#pragma once

using namespace System;

class AnnotationRecordOperator;
#include <cstdint>

namespace AnnotationTool {
	namespace NativeInteropServices {
		public ref class AnnotationRecordOperator
		{
		public:
			enum class DesiredAccess : uint32_t
			{
				Read,
				Write,
				Both
			};
			enum class CreationDisposition : uint32_t
			{
				OpenAlways,
				CreateAlways
			};
			AnnotationRecordOperator(System::String^ path, DesiredAccess desiredAccess, CreationDisposition creationDisposition);
			size_t Length();
			AnnotationTool::Data::Model::AnnotationRecord^ Get(size_t index);
			void Set(size_t index, AnnotationTool::Data::Model::AnnotationRecord^ record);
			void Resize(size_t n);
			~AnnotationRecordOperator();
		private:
			::AnnotationRecordOperator* _annotationOperator;
		};

	}
}