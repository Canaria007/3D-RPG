using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[System.Serializable]
//public class EventVector3 : UnityEvent<Vector3>{ }

public class MouseManager : Singleton<MouseManager>
{
    RaycastHit hitInfo;
    bool autoAttackTrigger;
    public GameObject autoAttackObject;
    //public EventVector3 OnMouseClicked;

    public Texture2D point, doorway, attack, target, arrow;

    public event Action<Vector3> OnMouseClicked;
    public event Action<GameObject> OnEnemyClicked;

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            switch (hitInfo.collider.tag) 
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Attackable":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        if(Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if(hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                autoAttackTrigger = false;
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            else if(hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                autoAttackTrigger = true;
                autoAttackObject = hitInfo.collider.gameObject;
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            else if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
        }
        else if(autoAttackTrigger && autoAttackObject)
        {
            OnEnemyClicked?.Invoke(autoAttackObject);
        }
    }

}
