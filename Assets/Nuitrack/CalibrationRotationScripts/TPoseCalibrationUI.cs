using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TPoseCalibrationUI : MonoBehaviour 
{
	[SerializeField]Text calibrationStatusText;
  [SerializeField]string startTooltip = "Stand in T-pose for calibration";
  [SerializeField]string tooltipAfterFirstCalibration = "";
  [SerializeField]string calibrationProcessFormat = "Calibration" + System.Environment.NewLine + "{0:0}%";

	float cooldown;
	static bool calibratedOnce = false;

  TPoseCalibration calibration;

	void Start () 
	{
		calibration = FindObjectOfType<TPoseCalibration>();
		calibrationStatusText.text = "";
		if (calibration != null)
		{
			cooldown = calibration.CalibrationTime;

			calibration.onStart += onStart;
			calibration.onProgress += onProgress;
			calibration.onSuccess += onSuccess;
			calibration.onFail += onFail;
		}

    calibrationStatusText.text = (calibratedOnce ? tooltipAfterFirstCalibration : startTooltip);
	}

	void OnDestroy()
	{
		if (calibration != null)
		{
			calibration.onStart -= onStart;
			calibration.onProgress -= onProgress;
			calibration.onSuccess -= onSuccess;
			calibration.onFail -= onFail;
		}
	}

	void onStart()
	{
		StopCoroutine(ResetTooltipOnCD());
    calibrationStatusText.text = string.Format(calibrationProcessFormat, 0f);
	}

	void onProgress (float progress)
	{
    calibrationStatusText.text = string.Format(calibrationProcessFormat, 100f * progress);
	}

	void onSuccess(Quaternion q)
	{
		if (calibrationStatusText != null) calibrationStatusText.text = "Calibration" + System.Environment.NewLine + "Done";
		calibratedOnce = true;
		StartCoroutine(ResetTooltipOnCD());
	}

	void onFail()
	{
		if (!calibratedOnce)
		{
      calibrationStatusText.text = startTooltip;
		}
    else
    {
      calibrationStatusText.text = tooltipAfterFirstCalibration;
    }
	}

	IEnumerator ResetTooltipOnCD()
	{
		yield return new WaitForSeconds(cooldown);
    calibrationStatusText.text = (calibratedOnce ? tooltipAfterFirstCalibration : startTooltip);
	}
}
