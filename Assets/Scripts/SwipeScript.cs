using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeScript : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    private Vector3 panelLocation;
    private float myheight;
    public float speed = 10.0f;
    public float microstep = 5;
    public float timetick = 0.01f;
    public GameObject RootObject;
    private RectTransform rt1;
    void Start()
    {
        panelLocation = transform.localPosition;
        myheight = GetComponent<RectTransform>().rect.height;
        rt1 = RootObject.GetComponent<RectTransform>();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        panelLocation = transform.localPosition;
    }
    public void OnDrag(PointerEventData data)
    {
        float difference = data.pressPosition.y - data.position.y;
        transform.localPosition = panelLocation - new Vector3(0, difference*1.2f, 0);
    }
    public void OnEndDrag(PointerEventData data)
    {
        panelLocation = transform.localPosition;
        if (GetComponent<RectTransform>().rect.height > rt1.rect.height -300 - 50)  StartCoroutine(SmoothMove(panelLocation, rt1.rect.height/2 - GetComponent<RectTransform>().rect.height / 2 - 60, GetComponent<RectTransform>().rect.height / 2 - rt1.rect.height/2 + 300));
        else StartCoroutine(SmoothMove(panelLocation, GetComponent<RectTransform>().rect.height / 2 - rt1.rect.height/2 + 200, rt1.rect.height/2 - GetComponent<RectTransform>().rect.height / 2 - 60));
    }
    IEnumerator SmoothMove(Vector3 loc,float a, float b)
    {
        if (loc.y < a)
        {
            Vector3 temp = transform.localPosition;
            while (temp.y < a)
            {
                temp = transform.localPosition;
                temp.y += Mathf.Abs(temp.y-a)*speed/100+microstep; 
                transform.localPosition = temp;
                yield return new WaitForSeconds(timetick);
            }
            panelLocation = transform.localPosition;
            yield break;

        }
        if (loc.y > b)
        {
            Vector3 temp = transform.localPosition;
            while (temp.y > b)
            {
                temp = transform.localPosition;
                temp.y -= Mathf.Abs(temp.y - b) * speed / 100 + microstep;
                transform.localPosition = temp;
                yield return new WaitForSeconds(timetick);
            }
            panelLocation = transform.localPosition;
            yield break;
        }

    }
    void Update()
    {
        
    }
}
