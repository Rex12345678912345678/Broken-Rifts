using System;
using System.Collections.Generic;

public class NumberFormatProviderImpl
{
	public string GetDurationFormatStandard(TimeSpan span, bool withAddition = true)
	{
		if (span.Days >= 14)
		{
			if (withAddition)
			{
				return DIContainerInfrastructure.GetLocaService().Tr("gen_time_weeks", new Dictionary<string, string> { 
				{
					"{value_1}",
					(span.Days / 7).ToString("0")
				} });
			}

			return (span.Days / 7).ToString("0");
		}
		if (span.Days >= 3)
		{
			if (withAddition)
			{
				return DIContainerInfrastructure.GetLocaService().Tr("gen_time_days", new Dictionary<string, string> { 
				{
					"{value_1}",
					span.Days.ToString("0")
				} });
			}
			return span.Days.ToString("0");
		}
		if (span.TotalSeconds < 0.0)
		{
			return DIContainerInfrastructure.GetLocaService().Tr("gen_time_over", "Finished!");
		}
		return span.Hours + span.Days * 24 + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
	}

	public string GetDurationFormatStandardDown(TimeSpan span, bool withAddition = true)
	{
		if (span.Days > 2)
		{
			if (withAddition)
			{
				return DIContainerInfrastructure.GetLocaService().Tr("event_teaser_timeleft_days", new Dictionary<string, string> { 
				{
					"{value_1}",
					span.Days.ToString("0")
				} });
			}
			return span.Days.ToString("0");
		}
		if (span.TotalSeconds < 0.0)
		{
			return DIContainerInfrastructure.GetLocaService().Tr("gen_time_over", "Finished!");
		}
		var value = span.Hours + span.Days * 24 + ":" + span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");
		return DIContainerInfrastructure.GetLocaService().Tr("event_teaser_timeleft", new Dictionary<string, string> { { "{value_1}", value } });
	}

	public string GetResourceAmountFormat(int value)
	{
		var text = value.ToString("0");
		var length = text.Length;
		for (var num = length - 1; num >= 1; num--)
		{
			if ((length - num) % 3 == 0)
			{
				text = text.Insert(num, DIContainerInfrastructure.GetLocaService().Tr("gen_thousandseperator", "."));
			}
		}
		return text;
	}

	public string GetBattleStatsFormat(float stat)
	{
		return stat.ToString("0");
	}

	public string GetBattleStatsFractionalFormat(float stat)
	{
		return stat.ToString("0.##");
	}
}
