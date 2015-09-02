using System;

namespace Moodle
{
	internal static class Reference
	{
		public const string CourseXPath = "//*[@class=\"coursename\"]";

		public const string EnrolledCourseXPath = "//*[@class=\"courses frontpage-course-list-enrolled\"]";

		public const string EnrolledCourseUrlXPath = "//@href";

		public const string CourseContentParseXpath = "//*[@class=\"activityinstance\"]";
	}
}