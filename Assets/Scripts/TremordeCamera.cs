using UnityEngine;
using System.Collections;

public class TremorDeCamera : MonoBehaviour
{
    // Isso cria um acesso global fácil para este script
    public static TremorDeCamera Instancia;
    
    private Vector3 posicaoOriginal;

    void Awake()
    {
        Instancia = this;
    }

    void Start()
    {
        // Salva a posição exata da câmera para ela voltar ao normal depois do tremor
        posicaoOriginal = transform.localPosition;
    }

    // Função que outros scripts vão chamar
    public void Tremer()
    {
        // Inicia o tremor durando 0.2 segundos com uma força de 0.3
        StartCoroutine(TremorCoroutine(0.2f, 0.3f));
    }

    IEnumerator TremorCoroutine(float duracao, float magnitude)
    {
        float tempoPassado = 0.0f;

        while (tempoPassado < duracao)
        {
            // Sorteia posições aleatórias X e Y
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Move a câmera violentamente
            transform.localPosition = new Vector3(x, y, posicaoOriginal.z);
            tempoPassado += Time.deltaTime;
            
            yield return null; // Espera o próximo frame
        }
        
        // Coloca a câmera de volta no centro quando acaba
        transform.localPosition = posicaoOriginal;
    }
}
