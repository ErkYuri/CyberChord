using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;

public class GerenciadorDeRitmo : MonoBehaviour
{
    public float bpm = 118f; 
    public float tempoPorBatida; 
    private float tempoInicialDaMusica; 
    public float posicaoAtualDaMusica; 
    public float batidaAtual; 
    private int batidaCheiaAnterior = 0;
    private AudioSource tocadorDeMusica; 

    public GameObject[] moldesRobos; 
    public GameObject[] moldesNotas; 

    public int vidaDaBase = 5; 
    public Slider barraVisual; 

    private float posicaoXBase = -7f;

    void Start()
    {
        tocadorDeMusica = GetComponent<AudioSource>();
        tempoPorBatida = 60f / bpm;
        tempoInicialDaMusica = (float)AudioSettings.dspTime;
        tocadorDeMusica.Play();

        if (barraVisual != null)
        {
            barraVisual.maxValue = vidaDaBase;
            barraVisual.value = vidaDaBase;
        }
    }

    void Update()
    {
        posicaoAtualDaMusica = (float)(AudioSettings.dspTime - tempoInicialDaMusica); 
        batidaAtual = posicaoAtualDaMusica / tempoPorBatida; 

        int batidaCheiaAtual = Mathf.FloorToInt(batidaAtual);

        if (batidaCheiaAtual > batidaCheiaAnterior)
        {
            int sorteio = Random.Range(0, moldesRobos.Length);
            Instantiate(moldesRobos[sorteio], new Vector3(8f, -1f, 0f), Quaternion.identity);
            Instantiate(moldesNotas[sorteio], new Vector3(8f, -3f, 0f), Quaternion.identity);
            batidaCheiaAnterior = batidaCheiaAtual;
        }

        // --- LÓGICA DE DANO POR CONTATO ---
        GameObject[] todosInimigos = GameObject.FindGameObjectsWithTag("Inimigo");
        foreach (GameObject inimigo in todosInimigos)
        {
            if (inimigo.transform.position.x <= posicaoXBase)
            {
                Destroy(inimigo);
                Debug.Log("DANO POR CONTATO! O robô atropelou o Pulse!");
                TomarDano();
            }
        }

        // --- INPUT DO JOGADOR ---
        bool apertouH = Keyboard.current.hKey.wasPressedThisFrame;
        bool apertouJ = Keyboard.current.jKey.wasPressedThisFrame;
        bool apertouK = Keyboard.current.kKey.wasPressedThisFrame;
        bool apertouL = Keyboard.current.lKey.wasPressedThisFrame;

        int totalDeTeclasApertadas = (apertouH ? 1 : 0) + (apertouJ ? 1 : 0) + (apertouK ? 1 : 0) + (apertouL ? 1 : 0);

        if (totalDeTeclasApertadas > 1)
        {
            // Punição por esmagar botões: O tiro falha, mas não tira vida.
            Debug.Log("TELA TREME! Sobrecarga por apertar múltiplos botões!");
        }
        else if (totalDeTeclasApertadas == 1)
        {
            if (apertouH) TentarAcertarFisicamente("NotaH");
            if (apertouJ) TentarAcertarFisicamente("NotaJ");
            if (apertouK) TentarAcertarFisicamente("NotaK");
            if (apertouL) TentarAcertarFisicamente("NotaL");
        }
    }

    // --- NOVA CHECAGEM FÍSICA E VISUAL ---
    void TentarAcertarFisicamente(string nomeDaNotaEsperada)
    {
        // 1. Pega TODAS as notas na tela
        GameObject[] todasAsNotas = GameObject.FindGameObjectsWithTag("Nota");
        GameObject notaAlvo = null;
        float menorDistanciaDaEsfera = 100f;

        // 2. Procura a nota DESSA COR que está mais perto da Esfera Azul (X = 0)
        foreach (GameObject nota in todasAsNotas)
        {
            if (nota.name.Contains(nomeDaNotaEsperada))
            {
                float distancia = Mathf.Abs(nota.transform.position.x - 0f);
                if (distancia < menorDistanciaDaEsfera)
                {
                    menorDistanciaDaEsfera = distancia;
                    notaAlvo = nota;
                }
            }
        }

        // 3. A Janela de Acerto Físico (1.2 unidades representa os 0.3 segundos na velocidade 4)
        if (notaAlvo != null && menorDistanciaDaEsfera <= 1.2f)
        {
            Debug.Log("HIT! Munição " + nomeDaNotaEsperada + " correta na Esfera! Fogo!");
            
            // Destrói a munição que estava na Esfera
            Destroy(notaAlvo); 

            // Destrói o robô mais perto do Pulse
            GameObject robo = ObterMaisProximoDoPulse("Inimigo");
            if (robo != null)
            {
                Destroy(robo);
            }
        }
        else
        {
            // A nota apertada não estava dentro da Esfera! 
            Debug.Log("MISS! TELA TREME! Arma falhou (Apertou fora de hora ou a cor errada).");
        }
    }

    void TomarDano()
{
    vidaDaBase--; // Subtrai a vida da Base agora!
    if (barraVisual != null) barraVisual.value = vidaDaBase;

    if (vidaDaBase > 0)
    {
        Debug.Log("DANO NA BASE! Estrutura danificada. Vida restante: " + vidaDaBase);
    }
    else if (vidaDaBase == 0)
    {
        Debug.Log("GAME OVER! A base foi destruída e a IA DeadBeat venceu!");
        tocadorDeMusica.Stop(); 
    }
}

    GameObject ObterMaisProximoDoPulse(string nomeDaTag)
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(nomeDaTag);
        GameObject objetoMaisProximo = null;
        float menorDistancia = 100f; 

        foreach(GameObject obj in objetos)
        {
            float distancia = Mathf.Abs(obj.transform.position.x - posicaoXBase); 
            if(distancia < menorDistancia)
            {
                menorDistancia = distancia;
                objetoMaisProximo = obj;
            }
        }
        return objetoMaisProximo;
    }
}