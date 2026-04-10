namespace ResumeApp.Services
{
    public class TimeHelper
    {
        private static readonly TimeZoneInfo IstZone =
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime UtcNow => DateTime.UtcNow;

        public static DateTime IstNow =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstZone);

        public static (DateTime startUtc, DateTime endUtc) GetTodayUtcRange()
        {
            var istNow = IstNow;

            var istStart = istNow.Date;
            var istEnd = istStart.AddDays(1);

            return (
                TimeZoneInfo.ConvertTimeToUtc(istStart, IstZone),
                TimeZoneInfo.ConvertTimeToUtc(istEnd, IstZone)
            );
        }
    }
}
