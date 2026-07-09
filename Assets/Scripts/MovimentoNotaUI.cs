using UnityEngine;

public class MovimentoNotaUI : MonoBehaviour
{
    // Em telas Full HD (1920px), uma velocidade de 800 a 1000 é ideal
    public float velocidadePixel = 800f; 

    void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if(rect != null)
        {
            // Move a nota para a esquerda baseada em pixels por segundo
            rect.anchoredPosition += Vector2.left * velocidadePixel * Time.deltaTime;
        }
    }
}
