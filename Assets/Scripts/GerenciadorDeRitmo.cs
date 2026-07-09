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
            
            Instantiate(moldesRobos[sorteio], new Vector3(8f, -1.3f, 0f), Quaternion.identity);

            float alturaDaLinha = 0f;
            if (sorteio == 0) alturaDaLinha = 290f;       
            else if (sorteio == 1) alturaDaLinha = 210f;  
            else if (sorteio == 2) alturaDaLinha = 130f;  
            else if (sorteio == 3) alturaDaLinha = 50f;   

            GameObject novaNota = Instantiate(moldesNotas[sorteio]);
            
            GameObject braco = GameObject.Find("BracoDaGuitarra");
            if(braco != null)
            {
                novaNota.transform.SetParent(braco.transform, false);
            }

            RectTransform rectNota = novaNota.GetComponent<RectTransform>();
            if(rectNota != null)
            {
                rectNota.anchoredPosition = new Vector2(1920f, alturaDaLinha);
            }

            batidaCheiaAnterior = batidaCheiaAtual;
        }

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

        bool apertouH = Keyboard.current.hKey.wasPressedThisFrame;
        bool apertouJ = Keyboard.current.jKey.wasPressedThisFrame;
        bool apertouK = Keyboard.current.kKey.wasPressedThisFrame;
        bool apertouL = Keyboard.current.lKey.wasPressedThisFrame;

        int totalDeTeclasApertadas = (apertouH ? 1 : 0) + (apertouJ ? 1 : 0) + (apertouK ? 1 : 0) + (apertouL ? 1 : 0);

        if (totalDeTeclasApertadas > 1)
        {
            Debug.Log("Sobrecarga! Punindo jogador...");
            // CHAMA O TREMOR AQUI
            if(TremorDeCamera.Instancia != null) TremorDeCamera.Instancia.Tremer(); 
        }
        else if (totalDeTeclasApertadas == 1)
        {
            // Removemos a exigência do nome do robô. Apenas a nota e o alvo importam agora!
            if (apertouH) TentarAcertarFisicamente("NotaH", "AlvoH"); 
            if (apertouJ) TentarAcertarFisicamente("NotaJ", "AlvoJ");     
            if (apertouK) TentarAcertarFisicamente("NotaK", "AlvoK");       
            if (apertouL) TentarAcertarFisicamente("NotaL", "AlvoL");     
        }
    }

    void TentarAcertarFisicamente(string nomeDaNota, string nomeDoAlvo)
    {
        GameObject alvo = GameObject.Find(nomeDoAlvo);
        if (alvo == null) return;

        float alvoX = alvo.transform.position.x; 

        GameObject[] todasAsNotas = GameObject.FindGameObjectsWithTag("Nota");
        if (todasAsNotas.Length == 0) return;

        GameObject notaAlvo = null;
        float menorDistancia = 10000f; 

        foreach (GameObject nota in todasAsNotas)
        {
            if (nota.name.Contains(nomeDaNota))
            {
                float distancia = Mathf.Abs(nota.transform.position.x - alvoX);
                if (distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    notaAlvo = nota;
                }
            }
        }

        if (notaAlvo != null && menorDistancia <= 70f)
        {
            Debug.Log($"✅ HIT! A nota {nomeDaNota} gerou energia! Destruindo ameaça mais próxima...");
            
            Destroy(notaAlvo); 
            
            // Agora o laser sempre foca no inimigo mais avançado no mapa
            GameObject roboAmeaca = ObterInimigoMaisProximoDaBase();
            if (roboAmeaca != null) Destroy(roboAmeaca);
        }
        else
        {
            Debug.Log($"❌ MISS! Fora do ritmo.");
            // Aciona a estática cinza
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
        }
    }

    void TomarDano()
    {
        // Aciona o alerta vermelho forte
        if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarDanoVermelho();

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

    // Função renomeada e simplificada: busca apenas quem está mais perto da base, sem checar nome
    GameObject ObterInimigoMaisProximoDaBase()
    {
        GameObject[] objetos = GameObject.FindGameObjectsWithTag("Inimigo");
        GameObject objetoMaisProximo = null;
        float menorDistancia = 1000f; 

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