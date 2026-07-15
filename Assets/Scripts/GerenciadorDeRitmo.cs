using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Essencial para trocar/reiniciar fases!

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
    public AudioClip somDanoBase; 
    [Range(0f, 1f)] public float volumeEfeitos = 0.4f; 
    public GameObject prefabExplosao; 

    [Header("Sistema de Pontos")]
    public TMP_Text textoScore;
    public TMP_Text textoCombo;
    private int pontuacao = 0;
    private int comboAtual = 0;
    private int multiplicador = 1;

    [Header("Telas de Fim de Jogo")]
    public GameObject painelFimDeJogo;
    public TMP_Text textoTituloFim;
    public TMP_Text textoScoreFinal;
    private bool jogoAtivo = true; // Controla se o jogo está rodando

    void Start()
    {
        Time.timeScale = 1f; // Garante que o tempo está normal ao iniciar
        tocadorDeMusica = GetComponent<AudioSource>();
        tempoPorBatida = 60f / bpm;
        tempoInicialDaMusica = (float)AudioSettings.dspTime;
        tocadorDeMusica.Play();

        if (barraVisual != null)
        {
            barraVisual.maxValue = vidaDaBase;
            barraVisual.value = vidaDaBase;
        }
        
        if (painelFimDeJogo != null) painelFimDeJogo.SetActive(false); // Esconde a tela no começo
        AtualizarTextosUI();
    }

    void Update()
    {
        // Se o jogo acabou, ignora todo o resto do Update!
        if (!jogoAtivo) return; 

        posicaoAtualDaMusica = (float)(AudioSettings.dspTime - tempoInicialDaMusica); 
        batidaAtual = posicaoAtualDaMusica / tempoPorBatida; 

        // --- CONDIÇÃO DE VITÓRIA ---
        // Se a música parou sozinha (não foi pausada) e já passou do começo
        if (!tocadorDeMusica.isPlaying && batidaAtual > 10f && vidaDaBase > 0)
        {
            VencerJogo();
        }

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
            QuebrarCombo();
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
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
        // ... (código mantido igual)
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
                if (distancia < menorDistancia) { menorDistancia = distancia; notaAlvo = nota; }
            }
        }

        if (notaAlvo != null && menorDistancia <= 70f)
        {
            Destroy(notaAlvo); 
            RegistrarAcerto(); 
            GameObject roboAmeaca = ObterInimigoMaisProximoDaBase();
            if (roboAmeaca != null) 
            {
                if (prefabExplosao != null) 
                {
                    GameObject efeitoExplosao = Instantiate(prefabExplosao, roboAmeaca.transform.position, Quaternion.identity);
                    Destroy(efeitoExplosao, 0.2f); 
                }
                Destroy(roboAmeaca);
            }
        }
        else
        {
            QuebrarCombo();
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
        }
    }

    void TomarDano()
    {
        QuebrarCombo();
        if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarDanoVermelho();
        if (somDanoBase != null) tocadorDeMusica.PlayOneShot(somDanoBase, volumeEfeitos);

        vidaDaBase--; 
        if (barraVisual != null) barraVisual.value = vidaDaBase;
        
        if (vidaDaBase <= 0)
        {
            PerderJogo();
        }
    }

    GameObject ObterInimigoMaisProximoDaBase()
    {
        // ... (código mantido igual)
        GameObject[] objetos = GameObject.FindGameObjectsWithTag("Inimigo");
        GameObject objetoMaisProximo = null;
        float menorDistancia = 1000f; 
        foreach(GameObject obj in objetos)
        {
            float distancia = Mathf.Abs(obj.transform.position.x - posicaoXBase); 
            if(distancia < menorDistancia) { menorDistancia = distancia; objetoMaisProximo = obj; }
        }
        return objetoMaisProximo;
    }

    void RegistrarAcerto() { /*... mantido...*/ comboAtual++; if (comboAtual >= 15) multiplicador = 4; else if (comboAtual >= 10) multiplicador = 3; else if (comboAtual >= 5) multiplicador = 2; else multiplicador = 1; pontuacao += 50 * multiplicador; AtualizarTextosUI(); }
    void QuebrarCombo() { /*... mantido...*/ comboAtual = 0; multiplicador = 1; AtualizarTextosUI(); }
    void AtualizarTextosUI() { /*... mantido...*/ if (textoScore != null) textoScore.text = "SCORE: " + pontuacao.ToString("000000"); if (textoCombo != null) { if (comboAtual > 0) textoCombo.text = "COMBO x" + multiplicador; else textoCombo.text = ""; } }

    // --- SISTEMA DE FIM DE JOGO ---
    void PerderJogo()
    {
        jogoAtivo = false;
        tocadorDeMusica.Stop();
        Time.timeScale = 0f; // Congela tudo (inimigos, notas)
        
        if (painelFimDeJogo != null)
        {
            painelFimDeJogo.SetActive(true);
            if (textoTituloFim != null) { textoTituloFim.text = "YOU LOSE"; textoTituloFim.color = Color.red; }
            if (textoScoreFinal != null) textoScoreFinal.text = "PONTUAÇÃO FINAL: " + pontuacao.ToString("000000");
        }
    }

    void VencerJogo()
    {
        jogoAtivo = false;
        Time.timeScale = 0f; // Congela a tela final
        
        if (painelFimDeJogo != null)
        {
            painelFimDeJogo.SetActive(true);
            if (textoTituloFim != null) { textoTituloFim.text = "YOU WIN!"; textoTituloFim.color = Color.cyan; }
            if (textoScoreFinal != null) textoScoreFinal.text = "PONTUAÇÃO FINAL: " + pontuacao.ToString("000000");
        }
    }

    // Funções para os botões chamarem
    public void ReiniciarFase()
    {
        Time.timeScale = 1f; // Descongela antes de reiniciar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarProMenu()
    {
        Time.timeScale = 1f; // Descongela antes de sair
        SceneManager.LoadScene("MenuInicial"); // O nome deve ser exatamente o da sua cena do Menu!
    }
}