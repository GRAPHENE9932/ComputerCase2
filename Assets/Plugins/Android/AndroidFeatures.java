package KlimSoft;

import android.content.Context;
import android.os.Vibrator;
import android.os.VibrationEffect;
import android.widget.Toast;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.app.Activity;
import android.content.pm.ApplicationInfo;
import java.util.List;

class AndroidFeatures
{
	private static Context context;
	private static Activity activity;
	private static Vibrator vibrator;

	public static int dialogButtonId = -1;

	public static void setContext(Context contextParam)
	{
		context = contextParam;
	}

	public static void setActivity(Activity activityParam)
	{
		activity = activityParam;
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

	public static void killProcess()
	{
		android.os.Process.killProcess(android.os.Process.myPid());
	}

	public static String getSignature()
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		try
		{
			Signature[] signatures = context
			.getPackageManager()
			.getPackageInfo(context.getPackageName(), PackageManager.GET_SIGNATURES)
			.signatures;

			return signatures[0].toCharsString();
		}
		catch (Exception e)
		{
			return null;
		}
	}

	public static void checkSignature(String trueSignature)
	{
		String currentSignature = getSignature();

		if (currentSignature == null)
			killProcess();

		if (currentSignature != trueSignature)
			killProcess();
	}

	public static void showDialogWithOneButton(String message, String title, String buttonText, boolean cancelable)
	{
		if (activity == null)
			throw new NullPointerException("Activity is not assigned.");

		AlertDialog.Builder builder = new AlertDialog.Builder(activity);

		builder.setMessage(message)
		.setTitle(title)
		.setCancelable(cancelable)
		.setNeutralButton(buttonText, new DialogInterface.OnClickListener()
		{
			public void onClick(DialogInterface dialog, int id)
			{
				dialogButtonId = id;
			}
		});

		builder.create().show();
	}

	public static String[] getPackageNames()
	{
		if (context == null)
			throw new NullPointerException("Context is not assigned.");

		PackageManager packageManager = context.getPackageManager();
		List<ApplicationInfo> packages = packageManager.getInstalledApplications(PackageManager.GET_META_DATA);
		String[] result = new String[packages.size()];

		for (int i = 0; i < packages.size(); i++)
			result[i] = packages.get(i).packageName;

		return result;
	}

	public static boolean isAppInstalled(String[] triggers)
	{
		String[] packageNames = getPackageNames();

		for (int i = 0; i < triggers.length; i++)
			for (int j = 0; j < packageNames.length; j++)
				if (packageNames[j] == triggers[i])
					return true;
		
		return false;
	}
}