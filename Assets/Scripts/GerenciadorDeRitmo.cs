using UnityEngine;
using UnityEngine.InputSystem; 
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GerenciadorDeRitmo : MonoBehaviour
{
    public float bpm = 118f; 
    public float tempoPorBatida; 
    public float posicaoAtualDaMusica; 
    public float batidaAtual; 
    private int batidaCheiaAnterior = 0;
    private AudioSource tocadorDeMusica; 

    public GameObject[] moldesRobos; 
    public GameObject[] moldesNotas; 

    // VIDA MUDADA PARA 10!
    public int vidaDaBase = 10; 
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
    private bool jogoAtivo = true; 

    [Header("Sprites do Protagonista")]
    public SpriteRenderer pulseRender; // Arrastaremos o Pulse aqui
    public Sprite spriteParado;        // Arte dele parado
    public Sprite[] spritesTocando;   // As 3 artes dele tocando

    [Header("Sistema de Pausa")]
    public GameObject painelPausa;
    public TMP_Text textoPausa;
    private bool jogoPausado = false;

    void Start()
    {
        Time.timeScale = 1f; 
        tocadorDeMusica = GetComponent<AudioSource>();
        tempoPorBatida = 60f / bpm;
        tocadorDeMusica.Play();

        if (barraVisual != null)
        {
            barraVisual.maxValue = vidaDaBase;
            barraVisual.value = vidaDaBase;
        }
        
        if (painelFimDeJogo != null) painelFimDeJogo.SetActive(false); 
        AtualizarTextosUI();
    }

    void Update()
    {
        // 1. O código verifica separadamente quem apertou o botão
        bool apertouPausaTeclado = Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame;
        bool apertouPausaControle = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
                            
        // 2. Enviamos "false" se foi teclado, e "true" se foi controle
        if (apertouPausaTeclado)
        {
            AlternarPausa(false); 
        }
        else if (apertouPausaControle)
        {
            AlternarPausa(true);
        }

        // 3. A mesma coisa para voltar pro menu
        bool apertouVoltarTeclado = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool apertouVoltarControle = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;

        if (jogoPausado && (apertouVoltarTeclado || apertouVoltarControle))
        {
            VoltarProMenu();
        }

        

        // 3. A BARREIRA: Se o jogo já acabou ou está pausado, ignora tudo que está abaixo!
        if (!jogoAtivo || jogoPausado) return; 

        // 4. Lógica de Ritmo e Batida (Agora usando o tempo interno da música para evitar dessincronização no Pause)
        posicaoAtualDaMusica = tocadorDeMusica.time; 
        batidaAtual = posicaoAtualDaMusica / tempoPorBatida; 

        // A vitória só é checada se passamos pela barreira de pausa lá em cima
        if (!tocadorDeMusica.isPlaying && batidaAtual > 10f && vidaDaBase > 0)
        {
            VencerJogo();
        }

        int batidaCheiaAtual = Mathf.FloorToInt(batidaAtual);

        if (batidaCheiaAtual > batidaCheiaAnterior)
        {
            int sorteio = Random.Range(0, moldesRobos.Length);

            // Preparamos a altura da nota e a altura do robô
            float alturaDaLinha = 0f;
            float alturaDoRobo = -1.3f; // Altura padrão para o Lobo, Gorila e Cobra (no chão)

            if (sorteio == 0) 
            {
                alturaDaLinha = 110f; // Linha H (Vermelha) - Lobo fica no chão
            }
            else if (sorteio == 1) 
            {
                alturaDaLinha = 40f;  // Linha J (Azul) - A Águia é Azul
                alturaDoRobo = 3.5f;  // Colocamos o voo para ela aqui!
            }
            else if (sorteio == 2) 
            {
                alturaDaLinha = -30f; // Linha K (Verde) - Cobra no chão
            }
            else if (sorteio == 3) 
            {
                alturaDaLinha = -100f; // Linha L (Amarela) - Gorila no chão
            }

            // Agora o robô nasce usando a 'alturaDoRobo' que configuramos acima
            Instantiate(moldesRobos[sorteio], new Vector3(8f, alturaDoRobo, 0f), Quaternion.identity);

            // A nota nasce normalmente
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
                inimigo.tag = "Untagged";
                ControleInimigo scriptInimigo = inimigo.GetComponent<ControleInimigo>();
                if(scriptInimigo != null) scriptInimigo.ExecutarAtaque();
                else Destroy(inimigo); // Segurança caso o script não esteja lá
                
                TomarDano();
            }
        }

        // --- SISTEMA DE CONTROLE (TECLADO + XBOX JUNTOS) ---
        // Ordem estilo Guitar Hero: LT (Vermelho) -> LB (Azul) -> RB (Verde) -> RT (Amarelo)

        // --- SISTEMA DE CONTROLE (SUA IDEIA: DIREITA PARA ESQUERDA) ---
        bool apertouH = (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) || 
                        (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame); // Vermelho (RT)

        bool apertouJ = (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame) || 
                        (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);  // Azul (RB)

        bool apertouK = (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame) || 
                        (Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame);   // Verde (LB)

        bool apertouL = (Keyboard.current != null && Keyboard.current.lKey.wasPressedThisFrame) || 
                        (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame);  // Amarelo (LT)
       
        int totalDeTeclasApertadas = (apertouH ? 1 : 0) + (apertouJ ? 1 : 0) + (apertouK ? 1 : 0) + (apertouL ? 1 : 0);

        if (totalDeTeclasApertadas > 1)
        {
            QuebrarCombo();
            if(EfeitoPiscarTela.Instancia != null) EfeitoPiscarTela.Instancia.PiscarEstaticaCinza();
        }
        else if (totalDeTeclasApertadas == 1)
        {
            int poseSorteada = Random.Range(0, spritesTocando.Length);
            pulseRender.sprite = spritesTocando[poseSorteada];
            
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

    void RegistrarAcerto() { comboAtual++; if (comboAtual >= 15) multiplicador = 4; else if (comboAtual >= 10) multiplicador = 3; else if (comboAtual >= 5) multiplicador = 2; else multiplicador = 1; pontuacao += 50 * multiplicador; AtualizarTextosUI(); }
    void QuebrarCombo() { comboAtual = 0; multiplicador = 1; AtualizarTextosUI(); }
    void AtualizarTextosUI() { if (textoScore != null) textoScore.text = "SCORE: " + pontuacao.ToString("000000"); if (textoCombo != null) { if (comboAtual > 0) textoCombo.text = "COMBO x" + multiplicador; else textoCombo.text = ""; } }

    void PerderJogo()
    {
        jogoAtivo = false;
        tocadorDeMusica.Stop();
        Time.timeScale = 0f; 
        
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
        Time.timeScale = 0f; 
        
        if (painelFimDeJogo != null)
        {
            painelFimDeJogo.SetActive(true);
            if (textoTituloFim != null) { textoTituloFim.text = "YOU WIN!"; textoTituloFim.color = Color.cyan; }
            if (textoScoreFinal != null) textoScoreFinal.text = "PONTUAÇÃO FINAL: " + pontuacao.ToString("000000");
        }
    }

    public void ReiniciarFase()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarProMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MenuInicial"); 
    }

    // Adicionamos o "bool naManete" aqui dentro dos parênteses
    void AlternarPausa(bool naManete = false)
    {
        if (!jogoAtivo) return; 

        jogoPausado = !jogoPausado; 

        if (jogoPausado)
        {
            Time.timeScale = 0f; 
            tocadorDeMusica.Pause(); 

            // A MÁGICA ACONTECE AQUI:
            if (textoPausa != null)
            {
                if (naManete)
                {
                    // \n\n significa "pular duas linhas" no texto!
                    textoPausa.text = "JOGO PAUSADO\n\nAperte START para continuar ou SELECT para o Menu.";
                }
                else
                {
                    textoPausa.text = "JOGO PAUSADO\n\nAperte P para continuar ou ESC para o Menu.";
                }
            }

            if (painelPausa != null) painelPausa.SetActive(true); 
        }
        else
        {
            Time.timeScale = 1f; 
            tocadorDeMusica.UnPause(); 
            if (painelPausa != null) painelPausa.SetActive(false); 
        }
    }
}