using UnityEngine;
using System.Collections;

public class ControleInimigo : MonoBehaviour
{
    public Sprite walk1;
    public Sprite walk2;
    public Sprite hitSprite;
    
    private SpriteRenderer sr;
    private bool estaMorto = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // Começa a rotina de trocar as asas/patas
        StartCoroutine(AnimarCaminhada());
    }

    IEnumerator AnimarCaminhada()
    {
        while (!estaMorto)
        {
            sr.sprite = walk1;
            yield return new WaitForSeconds(0.25f); // Troca a cada 0.25s
            sr.sprite = walk2;
            yield return new WaitForSeconds(0.25f);
        }
    }

    // Chamaremos isso quando ele tocar na base
    public void ExecutarAtaque()
    {
        estaMorto = true;
        StopAllCoroutines();
        sr.sprite = hitSprite;
        // Espera um pouquinho para o jogador ver o ataque antes de sumir
        Destroy(gameObject, 0.2f); 
    }
}