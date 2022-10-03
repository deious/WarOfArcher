using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public GameObject _animationObject = null;
    public GameObject _dieEffect = null;

    private void OnDeathAnimationEnd()
    {
        _animationObject.GetComponent<DefaultCharacter>().ReadyRespawn();
    }

    private void CallDeathEffect()
    {
        if (_dieEffect == null)
        {
            Debug.Log("DrownEffect가 없습니다");
        }
        _dieEffect.SetActive(true);
    }
}
