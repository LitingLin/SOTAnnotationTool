#pragma once

#if defined(COMPILING_LIB_ANNOTATION_RECORD_OPERATOR) && defined(_WINDLL)
#define LIB_ANNOTATION_RECORD_OPERATOR_EXPORTS __declspec(dllexport)
#else
#define LIB_ANNOTATION_RECORD_OPERATOR_EXPORTS __declspec(dllimport)
#endif

#include <cstdint>
#include <string>

class MATFile;

class LIB_ANNOTATION_RECORD_OPERATOR_EXPORTS AnnotationRecordOperator
{
public:
	enum class DesiredAccess : uint32_t
	{
		read = 0,
		write,
		both,
	};
	//	                        |                    When the file...
	// This argument:           |             Exists            Does not exist
	// -------------------------+------------------------------------------------------
	// CREATE_ALWAYS            |            Truncates             Creates
	// OPEN_ALWAYS     ===| does this |===>    Opens               Creates
	enum class CreationDisposition : uint32_t
	{
		open_always = 0,
		create_always
	};	
	AnnotationRecordOperator(const std::wstring &matFilePath, DesiredAccess desiredAccess, CreationDisposition creationDisposition);
	AnnotationRecordOperator(const AnnotationRecordOperator &) = delete;
	AnnotationRecordOperator(AnnotationRecordOperator &&) = delete;
	~AnnotationRecordOperator() noexcept(false);
	size_t getNumberOfRecords() const;
	bool get(size_t index, int *id, bool *labeled, int *x, int *y, int *w, int *h, bool *occlusion, bool *outOfView, std::wstring *path) const;
	void update(size_t index, int id, bool labeled, int x, int y, int w, int h, bool occlusion, bool outOfView, const std::wstring &path);
	void resize(size_t n);
private:
	MATFile * _matFile;
	void *_variable;
	bool _pendingUpdates;
};
