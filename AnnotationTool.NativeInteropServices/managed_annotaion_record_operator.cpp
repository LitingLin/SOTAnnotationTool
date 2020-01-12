#include "managed_annotation_record_operator.h"

#include <annotation_record_operation.h>

#include "utils.h"

#include <fmt/format.h>

namespace AnnotationTool {
	namespace NativeInteropServices {
		AnnotationRecordOperator::AnnotationRecordOperator(System::String^ path, DesiredAccess desiredAccess,
			CreationDisposition creationDisposition)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			_annotationOperator = new ::AnnotationRecordOperator(msclr::interop::marshal_as<std::wstring>(path), (::AnnotationRecordOperator::DesiredAccess)desiredAccess, (::AnnotationRecordOperator::CreationDisposition)creationDisposition);
			EXCEPTION_SAFE_EXECUTION_END
		}

		size_t AnnotationRecordOperator::Length()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			return _annotationOperator->size();
			EXCEPTION_SAFE_EXECUTION_END
		}

		Tuple<Data::Model::AnnotationRecord^, bool>^ AnnotationRecordOperator::Get(size_t index)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			bool isValidRecord = true;
			
			int id;
			bool labeled;
			int x, y, w, h;
			bool occlusion, outOfView;
			std::wstring path;
			if (!_annotationOperator->get(index, &id, &labeled, &x, &y, &w, &h, &occlusion, &outOfView, &path))
				throw gcnew System::Exception(msclr::interop::marshal_as<String^>(fmt::format(L"Failed to get record, index: {}", index)));

			if (id != index + 1)
				isValidRecord = false;
			
			return gcnew Tuple<Data::Model::AnnotationRecord^, bool>(gcnew Data::Model::AnnotationRecord(labeled, x - 1, y - 1, w, h, occlusion, outOfView, msclr::interop::marshal_as<String^>(path)), isValidRecord);

			EXCEPTION_SAFE_EXECUTION_END
		}

		void AnnotationRecordOperator::Set(size_t index, Data::Model::AnnotationRecord^ record)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			System::String^ managedPath = record->Path;
			_annotationOperator->set(index, index + 1, record->IsLabeled, record->X + 1, record->Y + 1, record->W, record->H, record->IsFullyOccluded, record->IsOutOfView,msclr::interop::marshal_as<std::wstring>(managedPath));
			EXCEPTION_SAFE_EXECUTION_END
		}

		void AnnotationRecordOperator::Resize(size_t n)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			_annotationOperator->resize(n);
			EXCEPTION_SAFE_EXECUTION_END
		}

		AnnotationRecordOperator::~AnnotationRecordOperator()
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			delete _annotationOperator;
			EXCEPTION_SAFE_EXECUTION_END
		}
	}
}

