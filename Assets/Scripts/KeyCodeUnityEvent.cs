using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyCodeUnityEvent : MonoBehaviour
{
    public enum keyPressType{
        Hold, Up, Down
    };

    [SerializeField] private KeyCode key;
    [SerializeField] private keyPressType type;
    [SerializeField] private UnityEvent eventCallback;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(type){
            case keyPressType.Hold:
                if(Input.GetKey(key)) eventCallback.Invoke();
                break;
            case keyPressType.Up:
                if(Input.GetKeyUp(key)) eventCallback.Invoke();
                break;
            case keyPressType.Down:
                if(Input.GetKeyDown(key)) eventCallback.Invoke();
                break;
        }        
    }
}
