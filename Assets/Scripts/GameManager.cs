using UnityEngine;
using UnityEngine.UI; // IMPORTANTE: Usamos 'UnityEngine.UI' para o Text Legacy
using System.Collections; // CORREÇÃO: Necessário para IEnumerator (Corotinas)
using System.Collections.Generic; // IMPORTANTE: Necessário para usar List<>

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // IMPORTANTE: A variável agora é do tipo 'Text'
    public Text textoDaPontuacao; 

    public int vidaInicial = 3;
    private int vidasAtuais;
    private int pontuacao = 0;
    [Header("Configuração de Vitória")]
    public int pontosParaVencer = 25;

    [Header("Spawner de Coletáveis")]
    public GameObject coletavelPrefab;
    public float intervaloSpawn = 2.5f;
    public float delayInicialColetavel = 1.5f;
    public Vector2 areaMinima = new Vector2(-7, -4);
    public Vector2 areaMaxima = new Vector2(7, 4);

    [Header("Spawner de Inimigos")]
    public GameObject inimigoNormalPrefab;
    public GameObject inimigoGrandePrefab;
    public float intervaloInimigo = 7f;
    private int contagemInimigos = 0;
    
    [Header("Sistema de Dificuldade")]
    public float tempoParaAumentarDificuldade = 30f;
    public float reducaoIntervaloSpawn = 0.1f;
    public float reducaoIntervaloInimigo = 0.2f;
    private float tempoDecorrido = 0f;

    [Header("UI")]
    public GameObject painelGameOver;
    public GameObject painelVitoria;
    public GameObject painelPause;
    public Text textoVidas;
    
    [Header("Botões do Painel de Pausa")]
    public UnityEngine.UI.Button botaoContinuar;
    public UnityEngine.UI.Button botaoVoltarMenu;
    
    private bool jogoPausado = false;

    void Start()
    {
        vidasAtuais = vidaInicial;
        AtualizarTextoVidas();

        if (textoDaPontuacao != null)
        {
            textoDaPontuacao.text = "Pontos: 0";
        }

        pontuacao = 0;
        
        pontosParaVencer = 25;
        
        Debug.Log("=== INÍCIO DO JOGO ===");
        Debug.Log("Total de pontos para vencer (forçado para 25): " + pontosParaVencer);
        Debug.Log("Painel de vitória atribuído? " + (painelVitoria != null));
        
        if(painelGameOver != null) painelGameOver.SetActive(false);
        if(painelVitoria != null) 
        {
            painelVitoria.SetActive(false);
            Debug.Log("Painel de vitória encontrado e desativado.");
        }
        else
        {
            Debug.LogError("Painel de vitória NÃO está atribuído no Inspector!");
        }
        if(painelPause != null) painelPause.SetActive(false);
        
        if (botaoContinuar != null)
        {
            botaoContinuar.onClick.AddListener(ContinuarJogo);
        }
        
        if (botaoVoltarMenu != null)
        {
            botaoVoltarMenu.onClick.AddListener(IrParaMenu);
        }

        StartCoroutine(SpawnRotina());
        StartCoroutine(SpawnInimigoRotina());
        
        // VERIFICAÇÃO DE LOAD DO MENU
        if (PlayerPrefs.GetInt("CarregarSave", 0) == 1)
        {
            Debug.Log("Flag 'CarregarSave' detectada! Carregando jogo...");
            CarregarJogo();
            // Reseta a flag para o próximo jogo
            PlayerPrefs.SetInt("CarregarSave", 0);
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            AlternarPause();
        }
        
        if (!jogoPausado)
        {
            tempoDecorrido += Time.deltaTime;
            
            if (tempoDecorrido >= tempoParaAumentarDificuldade)
            {
                AumentarDificuldade();
                tempoDecorrido = 0f;
            }
            
            if (pontuacao >= pontosParaVencer && (painelVitoria == null || !painelVitoria.activeSelf))
            {
                Debug.Log("VERIFICAÇÃO DE VITÓRIA NO UPDATE! Pontos: " + pontuacao);
                VencerJogo();
            }
        }
    }
    
    void AumentarDificuldade()
    {
        intervaloSpawn = Mathf.Max(1.5f, intervaloSpawn - reducaoIntervaloSpawn);
        intervaloInimigo = Mathf.Max(2f, intervaloInimigo - reducaoIntervaloInimigo);
        
        InimigoPatrulha[] inimigos = FindObjectsByType<InimigoPatrulha>(FindObjectsSortMode.None);
        foreach (InimigoPatrulha inimigo in inimigos)
        {
            inimigo.velocidade += 0.8f;
        }
    }

    public void AlternarPause()
    {
        if((painelGameOver != null && painelGameOver.activeSelf) || (painelVitoria != null && painelVitoria.activeSelf)) return;

        jogoPausado = !jogoPausado;

        if (jogoPausado)
        {
            Time.timeScale = 0;
            if(painelPause != null) painelPause.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            if(painelPause != null) painelPause.SetActive(false);
        }
    }
    
    public void ContinuarJogo()
    {
        jogoPausado = false;
        Time.timeScale = 1;
        if(painelPause != null) painelPause.SetActive(false);
    }

    IEnumerator SpawnRotina()
    {
        yield return new WaitForSeconds(delayInicialColetavel);
        
        while (true)
        {
            yield return new WaitForSeconds(intervaloSpawn);
            SpawnarColetavel();
        }
    }

    IEnumerator SpawnInimigoRotina()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloInimigo);
            SpawnarInimigo();
        }
    }

    void SpawnarColetavel()
    {
        if (coletavelPrefab != null)
        {
            Vector2 posicaoAleatoria = new Vector2(Random.Range(areaMinima.x, areaMaxima.x), Random.Range(areaMinima.y, areaMaxima.y));
            Instantiate(coletavelPrefab, posicaoAleatoria, Quaternion.identity);
        }
    }

    void SpawnarInimigo()
    {
        contagemInimigos++;
        GameObject prefabParaNascer = inimigoNormalPrefab;

        if (contagemInimigos % 7 == 0) 
        {
            prefabParaNascer = inimigoGrandePrefab;
            Debug.Log("INIMIGO GRANDE SPAWNADO!");
        }

        if (prefabParaNascer != null)
        {
            Vector2 posicaoAleatoria = new Vector2(Random.Range(areaMinima.x, areaMaxima.x), Random.Range(areaMinima.y, areaMaxima.y));
            Instantiate(prefabParaNascer, posicaoAleatoria, Quaternion.identity);
        }
    }
    
    void AumentarDificuldadePorPontos()
    {
        if (pontuacao == 5)
        {
            AumentarDificuldadeNivel(1);
        }
        else if (pontuacao == 10)
        {
            AumentarDificuldadeNivel(2);
        }
        else if (pontuacao == 15)
        {
            AumentarDificuldadeNivel(3);
        }
        else if (pontuacao == 20)
        {
            AumentarDificuldadeNivel(4);
        }
        else if (pontuacao == 23)
        {
            AumentarDificuldadeNivel(5);
        }
    }
    
    void AumentarDificuldadeNivel(int nivel)
    {
        intervaloSpawn = Mathf.Max(1.2f, intervaloSpawn - (0.25f * nivel));
        intervaloInimigo = Mathf.Max(3f, intervaloInimigo - (0.4f * nivel));
        
        InimigoPatrulha[] inimigos = FindObjectsByType<InimigoPatrulha>(FindObjectsSortMode.None);
        foreach (InimigoPatrulha inimigo in inimigos)
        {
            inimigo.velocidade += 0.4f * nivel;
        }
        
        Debug.Log("Dificuldade aumentada no nível " + nivel + "!");
    }

    public void AdicionarPonto()
    {
        pontuacao += 1;
        AtualizarPontuacao();
        
        Debug.Log("Pontuação atual: " + pontuacao + " / " + pontosParaVencer);
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.TocarSomColeta();
        }

        MovimentoJogador jogador = FindFirstObjectByType<MovimentoJogador>();
        if(jogador == null)
        {
             GameObject playerObj = GameObject.FindWithTag("Player");
             if(playerObj) jogador = playerObj.GetComponent<MovimentoJogador>();
        }

        if (jogador != null)
        {
            jogador.Crescer();
        }
        
        AumentarDificuldadePorPontos();

        if (pontuacao >= pontosParaVencer)
        {
            Debug.Log("=== CONDIÇÃO DE VITÓRIA ATINGIDA! ===");
            Debug.Log("Pontos: " + pontuacao + " / " + pontosParaVencer);
            VencerJogo();
        }
    }
    
    public void AdicionarPontoInimigo()
    {
        pontuacao += 2;
        AtualizarPontuacao();
        
        Debug.Log("Pontuação atual (inimigo): " + pontuacao + " / " + pontosParaVencer);
        
        if (pontuacao >= pontosParaVencer)
        {
            Debug.Log("=== CONDIÇÃO DE VITÓRIA ATINGIDA (via inimigo)! ===");
            Debug.Log("Pontos: " + pontuacao + " / " + pontosParaVencer);
            VencerJogo();
        }
    }
    
    void AtualizarPontuacao()
    {
        if (textoDaPontuacao != null)
        {
            textoDaPontuacao.text = "Pontos: " + pontuacao;
        }
    }

    public void ReceberDano()
    {
        if (vidasAtuais <= 0) return;
        
        vidasAtuais--;
        AtualizarTextoVidas();

        if (AudioManager.instance != null) AudioManager.instance.TocarSomDano();
        
        Debug.Log("Vida recebida! Vidas restantes: " + vidasAtuais);

        if (vidasAtuais <= 0)
        {
            GameOver();
        }
    }

    void AtualizarTextoVidas()
    {
        if(textoVidas != null)
        {
            textoVidas.text = "Vidas: " + vidasAtuais;
        }
    }

    void GameOver()
    {
        if (AudioManager.instance != null) AudioManager.instance.TocarMusicaDerrota();
        Debug.Log("GAME OVER!");
        if(painelGameOver != null)
        {
            painelGameOver.SetActive(true);
            Debug.Log("Painel de derrota ativado!");
        }
        else
        {
            Debug.LogError("Painel de derrota não está atribuído no Inspector!");
        }
        Time.timeScale = 0;
    }

    void VencerJogo()
    {
        if (jogoPausado) 
        {
            Debug.Log("Jogo já está pausado, ignorando vitória.");
            return;
        }

        if (AudioManager.instance != null) AudioManager.instance.TocarMusicaVitoria();
        
        Debug.Log("=== VENCER JOGO CHAMADO ===");
        Debug.Log("Pontuação: " + pontuacao);
        Debug.Log("Total necessário: " + pontosParaVencer);
        Debug.Log("Painel de vitória é null? " + (painelVitoria == null));
        
        if(painelVitoria != null)
        {
            Transform parent = painelVitoria.transform.parent;
            if (parent != null && !parent.gameObject.activeSelf)
            {
                Debug.Log("Ativando parent do painel (Canvas)...");
                parent.gameObject.SetActive(true);
            }
            
            Debug.Log("Tentando ativar painel de vitória...");
            painelVitoria.SetActive(true);
            
            Canvas canvas = painelVitoria.GetComponentInParent<Canvas>();
            if (canvas != null && !canvas.gameObject.activeSelf)
            {
                Debug.Log("Ativando Canvas...");
                canvas.gameObject.SetActive(true);
            }
            
            if (painelVitoria.activeSelf && painelVitoria.activeInHierarchy)
            {
                Debug.Log("✓ Painel de vitória ativado com sucesso!");
            }
            else
            {
                Debug.LogError("✗ Painel de vitória não está visível! activeSelf: " + painelVitoria.activeSelf + " activeInHierarchy: " + painelVitoria.activeInHierarchy);
            }
        }
        else
        {
            Debug.LogError("ERRO: Painel de vitória não está atribuído no Inspector do GameManager!");
            Debug.LogError("Por favor, arraste o GameObject 'PainelVitoria' para o campo 'Painel Vitoria' no Inspector!");
        }
        
        Time.timeScale = 0;
        jogoPausado = true;
    }

    public void ReiniciarJogo()
    {
        Time.timeScale = 1; // Despausa
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1;
        SalvarJogo(); 
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu"); 
    }

    // ... (Métodos de Spawn e Gameplay) ...

    // --- SISTEMA DE SAVE (JSON) ---

    [System.Serializable]
    public class SaveData
    {
        public int vidasSalvas;
        public int pontuacaoSalva;
        
        // Dados do Jogador
        public Vector2 posicaoJogador;
        public List<Vector2> historicoCobra;
        public int tamanhoCobra; // Inclui cabeça
        
        // Dados dos Inimigos
        public List<Vector2> posicoesInimigos;
        public List<string> tiposInimigos; // "Normal", "Grande" (baseado no nome do objeto)
    }

    public void SalvarJogo()
    {
        SaveData dados = new SaveData();
        dados.vidasSalvas = vidasAtuais;
        dados.pontuacaoSalva = pontuacao;
        
        // 1. Salvar Jogador
        MovimentoJogador jogador = FindFirstObjectByType<MovimentoJogador>();
        if(jogador != null)
        {
            dados.posicaoJogador = jogador.transform.position;
            dados.historicoCobra = jogador.GetHistorico();
            dados.tamanhoCobra = jogador.GetTamanho();
        }
        
        // 2. Salvar Inimigos
        dados.posicoesInimigos = new List<Vector2>();
        dados.tiposInimigos = new List<string>();
        
        InimigoPatrulha[] inimigos = FindObjectsByType<InimigoPatrulha>(FindObjectsSortMode.None);
        foreach(InimigoPatrulha inimigo in inimigos)
        {
            dados.posicoesInimigos.Add(inimigo.transform.position);
            // Identifica o tipo pelo nome do prefab (clone)
            if(inimigo.gameObject.name.Contains("Grande"))
            {
                dados.tiposInimigos.Add("Grande");
            }
            else
            {
                dados.tiposInimigos.Add("Normal");
            }
        }

        string json = JsonUtility.ToJson(dados);
        string caminhoArquivo = Application.persistentDataPath + "/savegame.json";

        System.IO.File.WriteAllText(caminhoArquivo, json);
        Debug.Log("Jogo Completo Salvo em: " + caminhoArquivo);
    }

    public void CarregarJogo()
    {
        string caminhoArquivo = Application.persistentDataPath + "/savegame.json";

        if (System.IO.File.Exists(caminhoArquivo))
        {
            string json = System.IO.File.ReadAllText(caminhoArquivo);
            SaveData dados = JsonUtility.FromJson<SaveData>(json);

            // 1. Aplica dados básicos
            vidasAtuais = dados.vidasSalvas;
            pontuacao = dados.pontuacaoSalva;
            AtualizarTextoVidas();
            if (textoDaPontuacao != null) textoDaPontuacao.text = "Pontos: " + pontuacao;

            // 2. Limpar Cena (Inimigos antigos)
            InimigoPatrulha[] inimigosExistentes = FindObjectsByType<InimigoPatrulha>(FindObjectsSortMode.None);
            foreach(var inimigo in inimigosExistentes) Destroy(inimigo.gameObject);
            
            // 3. Restaurar Jogador
            MovimentoJogador jogador = FindFirstObjectByType<MovimentoJogador>();
            if(jogador != null)
            {
                jogador.LimparCorpo(); // Reseta corpo atual
                jogador.transform.position = dados.posicaoJogador;
                
                // Restaura histórico ANTES de reconstruir o corpo
                if(dados.historicoCobra != null && dados.historicoCobra.Count > 0)
                {
                    jogador.SetHistorico(dados.historicoCobra);
                }
                
                // Recria os segmentos
                jogador.ReconstruirCorpo(dados.tamanhoCobra);
            }
            
            // 4. Restaurar Inimigos
            if (dados.posicoesInimigos != null)
            {
                for(int i=0; i < dados.posicoesInimigos.Count; i++)
                {
                    GameObject prefab = inimigoNormalPrefab;
                    if(i < dados.tiposInimigos.Count && dados.tiposInimigos[i] == "Grande")
                    {
                        prefab = inimigoGrandePrefab;
                    }
                    
                    if(prefab != null)
                    {
                        Instantiate(prefab, dados.posicoesInimigos[i], Quaternion.identity);
                    }
                }
            }
            
            Debug.Log("Jogo Completo Carregado!");
        }
        else
        {
            Debug.Log("Nenhum save encontrado. Começando novo jogo.");
        }
    }

    /* 
       NOTA PARA O START:
       Você deve decidir se o jogo SEMPRE carrega (ex: Continuar) ou se começa do zero.
       Como não temos um botão "Continuar" no Menu Principal ainda, vou deixar o CarregarJogo opcional.
       Mas como você pediu para salvar ao pausar, podemos carregar automaticamente no Start 
       SE quisermos persistência total. Vou deixar comentado no Start para você decidir.
    */
}