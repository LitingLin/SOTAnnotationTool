#include "managed_annotation_record_operator.h"

#define WIN32_LEAN_AND_MEAN
#include <msclr/marshal.h>
#include <msclr/marshal_cppstd.h>

#include <fmt/format.h>

#define EXCEPTION_SAFE_EXECUTION_BEGIN try \
	{
#define EXCEPTION_SAFE_EXECUTION_END } \
catch (std::exception & exp) \
{ \
	throw gcnew System::Exception(msclr::interop::marshal_as<String^>(exp.what())); \
} \
catch (...) \
{ \
	throw gcnew System::Exception(); \
}

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
			return _annotationOperator->getNumberOfRecords();
			EXCEPTION_SAFE_EXECUTION_END
		}

		AnnotationTool::Data::Model::AnnotationRecord^ AnnotationRecordOperator::Get(size_t index)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			int id;
			bool labeled;
			int x, y, w, h;
			bool occlusion, outOfView;
			std::wstring path;
			if (!_annotationOperator->get(index, &id, &labeled, &x, &y, &w, &h, &occlusion, &outOfView, &path))
				throw gcnew System::Exception(msclr::interop::marshal_as<String^>(fmt::format(L"Failed to get record, index: {}", index)));
			AnnotationTool::Data::Model::AnnotationRecord^ record = gcnew AnnotationTool::Data::Model::AnnotationRecord;
			record->IsLabeled = labeled;
			record->X = x;
			record->Y = y;
			record->W = w;
			record->H = h;
			record->IsFullyOccluded = occlusion;
			record->IsOutOfView = outOfView;
			record->Path = msclr::interop::marshal_as<String^>(path);

			return record;
			EXCEPTION_SAFE_EXECUTION_END
		}

		void AnnotationRecordOperator::Set(size_t index, AnnotationTool::Data::Model::AnnotationRecord^ record)
		{
			EXCEPTION_SAFE_EXECUTION_BEGIN
			System::String^ managedPath = record->Path;
			_annotationOperator->update(index, index + 1, record->IsLabeled, record->X, record->Y, record->W, record->H, record->IsFullyOccluded, record->IsOutOfView,msclr::interop::marshal_as<std::wstring>(managedPath));
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

