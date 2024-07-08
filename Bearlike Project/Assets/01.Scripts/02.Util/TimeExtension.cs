namespace Util
{
    public static class TimeExtension
    {
        public static string TimeString(this int minute)
        {
            int hour = minute / 60;
            minute %= 60;
            
            // 시간과 분을 "00:00" 형식의 문자열로 포맷팅
            return $"{hour}:{minute:00}";
        }

        public static string TimeString(this float minute) => ((int)minute).TimeString();
    }
}