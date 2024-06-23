using UnityEngine;

public class CardHighlighter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Card")))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (CheckCardBelongsToA(hitObject))
                {
                    HighlightObject(hitObject);
                }
            }
        }
    }
    private bool CheckCardBelongsToA(GameObject obj)
    {
        CardPersonagem cardPersonagem = obj.GetComponent<CardPersonagem>();
        if (cardPersonagem != null && cardPersonagem.cardBelongsToA)
        {
            return true;
        }
        CardMagia cardMagia = obj.GetComponent<CardMagia>();
        if (cardMagia != null && cardMagia.cardBelongsToA)
        {
            return true;
        }
        CardDefesa cardDefesa = obj.GetComponent<CardDefesa>();
        if (cardDefesa != null && cardDefesa.cardBelongsToA)
        {
            return true;
        }
        return false;
    }
    private void HighlightObject(GameObject obj)
    {
        Debug.Log("Highlight now");
    }
}
