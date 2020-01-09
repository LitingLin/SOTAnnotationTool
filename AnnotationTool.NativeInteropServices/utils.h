#pragma once

#define WIN32_LEAN_AND_MEAN
#include <msclr/marshal.h>
#include <msclr/marshal_cppstd.h>

#define EXCEPTION_SAFE_EXECUTION_BEGIN try \
	{
#define EXCEPTION_SAFE_EXECUTION_END } \
catch (std::exception & exp) \
{ \
	throw gcnew System::Exception(msclr::interop::marshal_as<System::String^>(exp.what())); \
} \
catch (...) \
{ \
	throw gcnew System::Exception(); \
}