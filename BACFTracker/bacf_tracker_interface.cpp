#include "bacf_tracker.h"

BACFTracker::BACFTracker() = default;

BACFTracker::~BACFTracker() = default;

void BACFTracker::initialize(unsigned char* image, int width, int height, int boundingBoxX, int boundingBoxY,
                             int boundingBoxWidth, int boundingBoxHeight)
{
	cv::Mat mat(height, width, CV_8UC3, image);
	_tracker.init(mat, cv::Rect(boundingBoxX, boundingBoxY, boundingBoxWidth, boundingBoxHeight));
}

std::tuple<int, int, int, int> BACFTracker::predict(unsigned char* image, int width, int height)
{
	cv::Mat mat(height, width, CV_8UC3, image);
	_tracker.track(mat);
	const cv::Rect rect = _tracker.getBBox().get_rect();
	return {rect.x, rect.y, rect.width, rect.height};
}
