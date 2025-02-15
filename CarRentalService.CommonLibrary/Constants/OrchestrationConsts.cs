namespace CarRentalService.CommonLibrary.Constants;
public static class OrchestrationConsts
{
    public const string InstanceId = nameof(InstanceId);
    public const string SetOperation = "Set";
    public const string Value = nameof(Value);

    public const string EveryMinuteSchedule = "0 * * * * *";
    public const string EveryMidnightSchedule = "0 0 0 * * *";
}
