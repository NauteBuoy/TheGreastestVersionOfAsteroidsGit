using System.Collections;
using UnityEngine;

//public class ImmunityControler : MonoBehaviour
//{
//    [Header("Immunity Flash Settings")]
//    public SpriteRenderer immunityVisual;
//    Coroutine immunityRoutine;

//    public void PlayImmunityFlash(float immunityDuration)
//    {
//        if (!immunityVisual)
//            return;

//        if (immunityRoutine != null)
//        {
//            StopCoroutine(immunityRoutine);
//        }
//        immunityRoutine = StartCoroutine(ImmunityFlashRoutine(immunityDuration));
//    }

//    IEnumerator ImmunityFlashRoutine(float immunityDuration)
//    {
//        immunityVisual.enabled = true;

//        Color c = immunityVisual.color;
//        c.a = 0f;
//        immunityVisual.color = c;

//        float fadeIn = immunityDuration * 0.25f;
//        float fadeOut = immunityDuration * 0.25f;

//        float timeElapsed = 0f;
//        while (timeElapsed < fadeIn)
//        {
//            timeElapsed += Time.deltaTime;
//            c.a = Mathf.Lerp(0f, 1f, timeElapsed / fadeIn);
//            immunityVisual.color = c;
//            yield return null;
//        }

//        yield return new WaitForSeconds(immunityDuration - fadeIn - fadeOut);

//        timeElapsed = 0f;
//        while (timeElapsed < fadeOut)
//        {
//            timeElapsed += Time.deltaTime;
//            c.a = Mathf.Lerp(1f, 0f, timeElapsed / fadeOut);
//            immunityVisual.color = c;
//            yield return null;
//        }

//        immunityVisual.enabled = false;
//    }
//}
