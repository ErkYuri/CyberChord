using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EfeitoPiscarTela : MonoBehaviour
{
    public static EfeitoPiscarTela Instancia;
    private Image imagemTela;

    void Awake()
    {
        Instancia = this;
        imagemTela = GetComponent<Image>();
    }

    // Chama a estática cinza quando o jogador erra a nota
    public void PiscarEstaticaCinza()
    {
        StopAllCoroutines();
        // Cor Cinza (R:0.5, G:0.5, B:0.5) com 15% de transparência (0.15f)
        StartCoroutine(PiscarRotina(new Color(0.5f, 0.5f, 0.5f, 0.15f))); 
    }

    // Chama o alerta vermelho quando a base toma dano
    public void PiscarDanoVermelho()
    {
        StopAllCoroutines();
        // Cor Vermelha (R:1.0, G:0.0, B:0.0) com 30% de transparência (0.3f) - mais visível
        StartCoroutine(PiscarRotina(new Color(1f, 0f, 0f, 0.1f))); 
    }

    // A rotina inteligente que aceita a cor que nós mandarmos
    IEnumerator PiscarRotina(Color corDesejada)
    {
        imagemTela.color = corDesejada;
        yield return new WaitForSeconds(0.1f);
        imagemTela.color = new Color(0f, 0f, 0f, 0f); // Volta a ficar invisível
    }
}