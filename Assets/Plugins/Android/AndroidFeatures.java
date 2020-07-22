package KlimSoft;

import android.content.Context;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.widget.Toast;

class AndroidFeatures
{
	private static Context context;

	private static Vibrator vibrator;

	public static void setContext(Context contextParam)
	{
		context = contextParam;
	}

	public static int getApiLevel()
	{
		return android.os.Build.VERSION.SDK_INT;
	}

	public static void vibrate(long milliseconds)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		try
		{
			if (vibrator == null)
				vibrator = (Vibrator)context.getSystemService("vibrator");

			if (getApiLevel() >= 26)
			{
				VibrationEffect effect = VibrationEffect.createOneShot(milliseconds, -1);
				vibrator.vibrate(effect);
			}
			else
			{
				vibrator.vibrate(milliseconds);
			}
		}
		catch(Exception e) { };
	}

	public static void vibrate(long[] intervals, int repeat)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		try
		{
			if (vibrator == null)
				vibrator = (Vibrator)context.getSystemService("vibrator");

			if (getApiLevel() >= 26)
			{
				VibrationEffect effect = VibrationEffect.createWaveform(intervals, repeat);
				vibrator.vibrate(effect);
			}
			else
			{
				vibrator.vibrate(intervals, repeat);
			}
		}
		catch(Exception e) { };
	}

	public static void makeToast(String text, int length)
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		Toast.makeText(context, text, length).show();
	}
}