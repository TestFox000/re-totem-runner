public class AnalyticsManager
{
	private static void Imp_AMLogEvent(string eventName)
	{
	}

	private static void Imp_AMLogEventParam(string eventName, string paramName, string paramVal)
	{
	}

	private static void Imp_AMLogEventParam2(string eventName, string paramName1, string paramVal1, string paramName2, string paramVal2)
	{
	}

	public static void LogEvent(string eventName)
	{
		Imp_AMLogEvent(eventName);
	}

	public static void LogEventWithParam(string eventName, string paramName, string paramVal)
	{
		Imp_AMLogEventParam(eventName, paramName, paramVal);
	}

	public static void LogEventWithParam2(string eventName, string paramName1, string paramVal1, string paramName2, string paramVal2)
	{
		Imp_AMLogEventParam2(eventName, paramName1, paramVal1, paramName2, paramVal2);
	}
}
