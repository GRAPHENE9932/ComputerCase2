package KlimSoft.AndroidFeatures;

import android.content.Context;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.widget.Toast;

class AndroidFeatures
{
	private static Context context;

	private static Vibrator vibrator

	public static void setContext(Context context)
	{
		this.context = context;
	}

	public static int getApiLevel()
	{
		return android.os.Build$VERSION.SDK_INT;
	}

	public static void Vibrate(long milliseconds)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		try
		{
			if (vibrator == null)
				vibrator = context.getSystemService("vibrator");

			if (getApiLevel() >= 26)
			{
				VibrationEffect effect = new VibrationEffect.createOneShot(milliseconds, -1);
				vibrator.vibrate(effect);
			}
			else
			{
				vibrator.vibrate(milliseconds);
			}
		}
		catch { };
	}

	public static void Vibrate(long[] intervals, int repeat)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		try
		{
			if (vibrator == null)
				vibrator = context.getSystemService("vibrator");

			if (getApiLevel() >= 26)
			{
				VibrationEffect effect = new VibrationEffect.createOneShot(intervals, repeat);
				vibrator.vibrate(effect);
			}
			else
			{
				vibrator.vibrate(intervals, repeat);
			}
		}
		catch { };
	}

	public static void MakeToast(string text, int length)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		Toast.makeText(context, text, length).show();
	}
}