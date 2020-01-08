#include "exports.h"

#include "kcf.h"

extern "C"{
	void *createBACFTracker()
	{
		try 
		{
			KCF_Tracker *kcf_tracker = new KCF_Tracker();
			return kcf_tracker;
		}
		catch (...)
		{
			return nullptr;
		}
	}
	bool initializeBACFTracker(void *handle, unsigned char *data, int width, int height, double boundingBoxX, double boundingBoxY, double boundingBoxW, double boundingBoxH)
	{
		try 
		{
			KCF_Tracker *kcf_tracker = (KCF_Tracker*)handle;
			cv::Mat mat(height, width, CV_8UC3, data);
			kcf_tracker->init(mat, cv::Rect(boundingBoxX, boundingBoxY, boundingBoxW, boundingBoxH));
			return true;
		}
		catch (...)
		{
			return false;
		}
	}
	bool predictBACFTracker(void *handle, unsigned char *image, int width, int height, double *boundingBoxX, double *boundingBoxY, double *boundingBoxW, double *boundingBoxH)
	{
		try 
		{
			KCF_Tracker *kcf_tracker = (KCF_Tracker*)handle;
			cv::Mat mat(height, width, CV_8UC3, image);
			kcf_tracker->track(mat);
			const cv::Rect rect = kcf_tracker->getBBox().get_rect();
			*boundingBoxX = rect.x;
			*boundingBoxY = rect.y;
			*boundingBoxW = rect.width;
			*boundingBoxH = rect.height;
			return true;
		}
		catch (...)
		{
			return false;
		}
	}
	bool destroyBACFTracker(void *handle)
	{
		try
		{
			KCF_Tracker *kcf_tracker = (KCF_Tracker*)handle;
			delete kcf_tracker;
			return true;
		}
		catch (...)
		{
			return false;
		}
	}
}