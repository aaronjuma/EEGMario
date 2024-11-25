using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// This is a dumb model created by ChatGPT-4. It is not a good model, but it is a model.
/// It kinda works, but it has not been validated by any neurologist or neurotechnologist.
/// This model was created in desperation, lacking any interpreted values given by the Muse in and of itself.
/// If you have a better or more accurate open source model, please send me an e-mail to theneurogamedev@gmail.com
/// </summary>
public class StupidModel
{
    private float buffer = 0.1f;


    private DateTime _previousCalculationTime = DateTime.MinValue;

    public float GetCalm(ChannelData alpha, ChannelData beta, ChannelData delta, ChannelData theta, ChannelData gamma)
    {
        double calmnessScore = (alpha.TP9 + alpha.AF7 + alpha.AF8 + alpha.TP10 +
                                -beta.TP9 - beta.AF7 - beta.AF8 - beta.TP10 +
                                delta.TP9 + delta.AF7 + delta.AF8 + delta.TP10 +
                                theta.TP9 + theta.AF7 + theta.AF8 + theta.TP10) / 16f;

        if (calmnessScore == 0)
        {
            return 0;
        }

        return Mathf.Clamp(((float)calmnessScore + buffer), 0f, 1f);

        //return Mathf.Abs((float)calmnessScore);
    }

    public float GetFocus(ChannelData alpha, ChannelData beta, ChannelData delta, ChannelData theta, ChannelData gamma)
    {
        double focusScore = (beta.AF7 + beta.AF8 + beta.TP9 + beta.TP10 +
            gamma.AF7 + gamma.AF8 + gamma.TP9 + gamma.TP10 +
            -delta.AF7 - delta.AF8 - delta.TP9 - delta.TP10 +
            -theta.AF7 - theta.AF8 - theta.TP9 - theta.TP10) / 16f;

        if (focusScore == 0)
        {
            return 0;
        }

        return Mathf.Clamp(((float)focusScore + buffer), 0f, 1f);

        //return Mathf.Abs((float)focusScore);
    }

    public float GetFlow(ChannelData alpha, ChannelData beta, ChannelData delta, ChannelData theta, ChannelData gamma)
    {
        double flowScore = (alpha.TP9 + alpha.AF7 + alpha.AF8 + alpha.TP10 +
                            gamma.TP9 + gamma.AF7 + gamma.AF8 + gamma.TP10 -
                            beta.TP9 - beta.AF7 - beta.AF8 - beta.TP10 -
                            delta.TP9 - delta.AF7 - delta.AF8 - delta.TP10 -
                            theta.TP9 - theta.AF7 - theta.AF8 - theta.TP10) / 20f;

        if (flowScore == 0)
        {
            return 0;
        }

        return Mathf.Clamp(((float)flowScore + buffer), 0f, 1f);

        //return Mathf.Abs((float)flowScore);
    }

    public float GetPPGRate(PPGData ppg)
    {
        var currentTime = DateTime.Now;

        var netInfraredSignal = ppg.infrared - ppg.ambient;
        var netRedSignal = ppg.red - ppg.ambient;

        var maxSignal = Math.Max(netInfraredSignal, netRedSignal);

        double ppgRate = 0;

        // Calculate only if peaks are detected
        if (maxSignal > 0)
        {
            var timeDifferenceInSeconds = (currentTime - _previousCalculationTime).TotalSeconds;
            var frequencyInHz = 1 / timeDifferenceInSeconds;

            // Heart rate is calculated as beats per minute
            ppgRate = frequencyInHz * 60;

            // Update the last calculation time
            _previousCalculationTime = currentTime;
        }

        return (float)ppgRate;

    }
}