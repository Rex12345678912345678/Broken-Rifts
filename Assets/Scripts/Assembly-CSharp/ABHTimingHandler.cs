public class ABHTimingHandler
{
	public int ProcessTimeFromSkynestTimeService(string jsonWebResponse)
	{
		if (jsonWebResponse == null)
		{
			return 0;
		}
		var result = 0;
		foreach (var item in SimpleJsonConverter.DecodeJsonDict(jsonWebResponse))
		{
			if (item.Key.ToLower().Equals("time"))
			{
				DebugLog.Log(item.Value.ToString());
				if (int.TryParse(item.Value.ToString(), out result))
				{
					break;
				}
			}
		}
		return result;
	}
}
