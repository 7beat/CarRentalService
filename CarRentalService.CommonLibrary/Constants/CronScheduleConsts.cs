namespace CarRentalService.CommonLibrary.Constants;
public static class CronScheduleConsts
{
    public const string EveryMinuteSchedule = "0 * * * * *";
    public const string EveryMidnightSchedule = "0 0 0 * * *";
    public const string Never = "0 0 0 31 2 *";
}
