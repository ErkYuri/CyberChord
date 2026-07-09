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
    public GameObject[] moldesNotas; // ATENÇÃO: Agora devem ser os novos Prefabs de UI!

    public int vidaDaBase = 5; 
    public Slider barraVisual; 

    // A borda de colisão da base onde os robôs dão dano
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

        // --- SISTEMA DE GERAÇÃO (SPAWNER) ---
        if (batidaCheiaAtual > batidaCheiaAnterior)
        {
            int sorteio = Random.Range(0, moldesRobos.Length);
            
            // 1. Cria o robô no cenário 2D marchando em direção à base (Y ajustado para a superfície)
            Instantiate(moldesRobos[sorteio], new Vector3(8f, -1.3f, 0f), Quaternion.identity);

            // 2. Define a altura (Y) da nota na UI com base nas suas novas coordenadas
            float alturaDaLinha = 0f;
            if (sorteio == 0) alturaDaLinha = 290f;       // Linha H
            else if (sorteio == 1) alturaDaLinha = 210f;  // Linha J
            else if (sorteio == 2) alturaDaLinha = 130f;  // Linha K
            else if (sorteio == 3) alturaDaLinha = 50f;   // Linha L

            // 3. Cria a nota visual da Interface
            GameObject novaNota = Instantiate(moldesNotas[sorteio]);
            
            // 4. Encontra o Braço da Guitarra na tela e coloca a nota dentro dele
            GameObject braco = GameObject.Find("BracoDaGuitarra");
            if(braco != null)
            {
                novaNota.transform.SetParent(braco.transform, false);
            }

            // 5. Posiciona a nota lá na direita do ecrã (X = 1920) e na altura certa
            RectTransform rectNota = novaNota.GetComponent<RectTransform>();
            if(rectNota != null)
            {
                rectNota.anchoredPosition = new Vector2(1920f, alturaDaLinha);
            }

            batidaCheiaAnterior = batidaCheiaAtual;
        }

        // --- LÓGICA DE DANO POR CONTATO (ROBÔS VS BASE) ---
        GameObject[] todosInimigos = GameObject.FindGameObjectsWithTag("Inimigo");
        foreach (GameObject inimigo in todosInimigos)
        {
            if (inimigo.transform.position.x <= posicaoXBase)
            {
                Destroy(inimigo);
                Debug.Log("DANO POR CONTATO! O robô atingiu a Base Defensiva!");
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
            // Punição por esmagar botões
            Debug.Log("TELA TREME! Sobrecarga por apertar múltiplos botões!");
        }
        else if (totalDeTeclasApertadas == 1)
        {
            // O valor 250f representa a coordenada X dos seus Alvos na tela
            if (apertouH) TentarAcertarFisicamente("NotaH", 250f);
            if (apertouJ) TentarAcertarFisicamente("NotaJ", 250f);
            if (apertouK) TentarAcertarFisicamente("NotaK", 250f);
            if (apertouL) TentarAcertarFisicamente("NotaL", 250f);
        }
    }

    // --- NOVA CHECAGEM FÍSICA E VISUAL NA UI ---
    void TentarAcertarFisicamente(string nomeDaNotaEsperada, float posicaoXDoAlvo)
    {
        GameObject[] todasAsNotas = GameObject.FindGameObjectsWithTag("Nota");
        GameObject notaAlvo = null;
        
        // A distância agora é medida em Pixels
        float menorDistanciaDoAlvo = 10000f; 

        // Encontra a nota exata que o jogador tentou acertar
        foreach (GameObject nota in todasAsNotas)
        {
            if (nota.name.Contains(nomeDaNotaEsperada))
            {
                RectTransform rect = nota.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Compara a posição X da nota com a posição X do alvo (250)
                    float distancia = Mathf.Abs(rect.anchoredPosition.x - posicaoXDoAlvo);
                    if (distancia < menorDistanciaDoAlvo)
                    {
                        menorDistanciaDoAlvo = distancia;
                        notaAlvo = nota;
                    }
                }
            }
        }

        // Janela de Acerto de 60 pixels (Pode ajustar para mais fácil ou mais difícil depois)
        if (notaAlvo != null && menorDistanciaDoAlvo <= 60f)
        {
            Debug.Log("HIT! Munição " + nomeDaNotaEsperada + " conectou no tempo perfeito!");
            
            // Destrói a nota da interface
            Destroy(notaAlvo); 

            // Atira o laser da guitarra e destrói a maior ameaça (robô mais perto)
            GameObject roboAmeaca = ObterMaisProximoDaBase("Inimigo");
            if (roboAmeaca != null)
            {
                Destroy(roboAmeaca);
            }
        }
        else
        {
            Debug.Log("MISS! Arma falhou (Fora do ritmo ou tecla errada).");
        }
    }

    void TomarDano()
    {
        vidaDaBase--; 
        if (barraVisual != null) barraVisual.value = vidaDaBase;
        
        if (vidaDaBase > 0)
        {
            Debug.Log("DANO! Vida restante: " + vidaDaBase);
        }
        else if (vidaDaBase <= 0)
        {
            Debug.Log("GAME OVER! A IA DeadBeat venceu!");
            tocadorDeMusica.Stop(); 
        }
    }

    GameObject ObterMaisProximoDaBase(string nomeDaTag)
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag(nomeDaTag);
        GameObject objetoMaisProximo = null;
        float menorDistancia = 1000f; 

        foreach(GameObject obj in objetos)
        {
            // Mede qual robô está mais perto da Base Defensiva
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