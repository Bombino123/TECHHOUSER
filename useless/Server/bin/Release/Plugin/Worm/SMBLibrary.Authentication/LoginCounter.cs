using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SMBLibrary.Authentication;

[ComVisible(true)]
public class LoginCounter
{
	public class LoginEntry
	{
		public DateTime LoginWindowStartDT;

		public int NumberOfAttempts;
	}

	private int m_maxLoginAttemptsInWindow;

	private TimeSpan m_loginWindowDuration;

	private Dictionary<string, LoginEntry> m_loginEntries = new Dictionary<string, LoginEntry>();

	public LoginCounter(int maxLoginAttemptsInWindow, TimeSpan loginWindowDuration)
	{
		m_maxLoginAttemptsInWindow = maxLoginAttemptsInWindow;
		m_loginWindowDuration = loginWindowDuration;
	}

	public bool HasRemainingLoginAttempts(string userID)
	{
		return HasRemainingLoginAttempts(userID, incrementCount: false);
	}

	public bool HasRemainingLoginAttempts(string userID, bool incrementCount)
	{
		lock (m_loginEntries)
		{
			if (m_loginEntries.TryGetValue(userID, out var value))
			{
				if (value.LoginWindowStartDT.Add(m_loginWindowDuration) >= DateTime.UtcNow)
				{
					if (incrementCount)
					{
						value.NumberOfAttempts++;
					}
				}
				else
				{
					if (!incrementCount)
					{
						return true;
					}
					value.LoginWindowStartDT = DateTime.UtcNow;
					value.NumberOfAttempts = 1;
				}
			}
			else
			{
				if (!incrementCount)
				{
					return true;
				}
				value = new LoginEntry();
				value.LoginWindowStartDT = DateTime.UtcNow;
				value.NumberOfAttempts = 1;
				m_loginEntries.Add(userID, value);
			}
			return value.NumberOfAttempts < m_maxLoginAttemptsInWindow;
		}
	}
}
