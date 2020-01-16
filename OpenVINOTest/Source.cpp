#include <inference_engine.hpp>

#include <iostream>

int main()
{
	InferenceEngine::Core core;
	auto devices = core.GetAvailableDevices();
	for (auto &device : devices)
	{
		std::cout << device << std::endl;
	}
	return 0;
}