namespace SFA.DAS.Forecasting.Commitments.Functions;

public static class EndpointNames
{
    private const string Prefix = "SFA.DAS.Fcast";
    
    public const string ApprenticeshipStopped = $"{Prefix}.{nameof(ApprenticeshipStopped)}";
    public const string ApprenticeshipCompletionDateUpdated = $"{Prefix}.{nameof(ApprenticeshipCompletionDateUpdated)}";
    public const string ApprenticeshipCompletedEvent = $"{Prefix}.{nameof(ApprenticeshipCompletedEvent)}";
    public const string ApprenticeshipStopDateChanged = $"{Prefix}.{nameof(ApprenticeshipStopDateChanged)}";

}