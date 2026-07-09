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
    private float posicaoXBase = -10f;

    [Header("Efeitos Sonoros e Visuais")]
    public AudioClip somAcerto;
    public AudioClip somErro;
    public AudioClip somDanoBase;
    [Range(0f, 1f)] public float volumeEfeitos = 0.4f; // 0.4 garante que seja mais baixo que a música
    public GameObject prefabExplosao; // Onde vai entrar a arte do seu colega

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
            if(braco != null) novaNota.transform.SetParent(braco.transform, false);

            RectTransform rectNota = novaNota.GetComponent<RectTransform>();
            if(rectNota != null) rectNota.anchoredPosition = new Vector2(1920f, alturaDaLinha);

            batidaCheiaAnterior = batidaCheiaAtual;
        }

        GameObject[] todosInimigos = GameObject.FindGameObjectsWithTag("Inimigo");
        foreach (GameObject inimigo in todosInimigos)
        {
            if (inimigo.transform.position.x <= posicaoXBase)
            {
                Destroy(inimigo);
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
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
            // Toca som de erro bem baixinho (metade do volume dos efeitos normais)
            if (somErro != null) tocadorDeMusica.PlayOneShot(somErro, volumeEfeitos * 0.5f);
        }
        else if (totalDeTeclasApertadas == 1)
        {
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
            Destroy(notaAlvo); 
            
            // Toca o som de Acerto
            if (somAcerto != null) tocadorDeMusica.PlayOneShot(somAcerto, volumeEfeitos);
            
            GameObject roboAmeaca = ObterInimigoMaisProximoDaBase();
            if (roboAmeaca != null) 
            {
                // Cria a explosão EXATAMENTE na posição onde o robô estava
                if (prefabExplosao != null) 
                {
                    Instantiate(prefabExplosao, roboAmeaca.transform.position, Quaternion.identity);
                }
                Destroy(roboAmeaca);
            }
        }
        else
        {
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
            // Toca som de erro
            if (somErro != null) tocadorDeMusica.PlayOneShot(somErro, volumeEfeitos * 0.5f);
        }
    }

    void TomarDano()
    {
        if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarDanoVermelho();
        
        // Toca som de dano na base
        if (somDanoBase != null) tocadorDeMusica.PlayOneShot(somDanoBase, volumeEfeitos);

        vidaDaBase--; 
        if (barraVisual != null) barraVisual.value = vidaDaBase;
        
        if (vidaDaBase <= 0)
        {
            Debug.Log("GAME OVER!");
            tocadorDeMusica.Stop(); 
        }
    }

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