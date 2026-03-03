using System.Globalization;

namespace CohesiveRP.Common.Utils
{
    public static class DateTimeUtils
    {
        public static string TryConvertToDateFormat(string dateToConvertFormat, string formatToConvert = null, string outputDateFormat = "yyyy-MM-dd")
        {
            if (string.IsNullOrWhiteSpace(dateToConvertFormat))
                return dateToConvertFormat;

            if (!string.IsNullOrWhiteSpace(formatToConvert))
            {
                // try to convert using the provided format
                if (DateTime.TryParseExact(dateToConvertFormat, formatToConvert, null, DateTimeStyles.None, out DateTime result))
                {
                    return result.ToString(outputDateFormat);
                }
            }

            if (DateTime.TryParse(dateToConvertFormat, out DateTime convertionResult))
            {
                return convertionResult.ToString(outputDateFormat);
            }

            return dateToConvertFormat;
        }
    }
}
