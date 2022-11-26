using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    public static class Utils
    {
        public static IEnumerator WaitingForCurrentAnimation(Animator animator,
            System.Action callback, float earlyExit = 0f,string waitForAnimName=null,bool stopAfterAnim=false)

        {
            if (stopAfterAnim)
            {
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(animator.GetAnimatorTransitionInfo(0).duration);
                yield return new WaitForEndOfFrame();
                yield return new WaitUntil(()=>animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f);
            }
            else
            {
                yield return new WaitUntil(() =>
                    animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == waitForAnimName);
                yield return new WaitForSeconds(
                    animator.GetCurrentAnimatorStateInfo(0).length - earlyExit);
            }
            callback();
        }

    }
}

