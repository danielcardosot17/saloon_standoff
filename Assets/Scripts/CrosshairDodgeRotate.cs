using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairDodgeRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private bool isActive = false;
    private Image image;
    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        image = GetComponent<Image>();
        image.enabled = isActive;
    }

    // Update is called once per frame
    void Update()
    {
        if(BattleSystem.Instance.LocalPlayer != null)
        {
            if(BattleSystem.Instance.LocalPlayer.ChosenAction == PlayerActions.DODGE && BattleSystem.Instance.BattleState == BattleState.COUNTDOWN)
            {
                if(!isActive)
                {
                    isActive = true;
                    image.enabled = isActive;
                }
                transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
            }
            else{
                isActive = false;
                image.enabled = isActive;
            }
        }
    }
}
