namespace SFA.DAS.Forecasting.Triggers;

public static class EndpointNames
{
   private const string Prefix = "SFA.DAS.Fcast.Jobs";

   public const string EmployerLevyDataRefreshed = $"{Prefix}.{nameof(EmployerLevyDataRefreshed)}";
   public const string FundsExpired = $"{Prefix}.{nameof(FundsExpired)}";
   public const string PaymentDataRefreshed = $"{Prefix}.{nameof(PaymentDataRefreshed)}";
}