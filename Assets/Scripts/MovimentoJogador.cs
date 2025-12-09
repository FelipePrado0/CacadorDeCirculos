using UnityEngine;
using System.Collections.Generic;

public class MovimentoJogador : MonoBehaviour
{
    public float velocidade = 5f;
    public GameObject corpoPrefab; 
    public int gap = 10; // Distância em frames/updates entre cada pedaço do corpo

    private Rigidbody2D rb;
    private Vector2 direcaoAtual = Vector2.right; 
    private Vector2 proximaDirecao = Vector2.right;

    private List<Transform> corpo = new List<Transform>();
    private List<Vector2> historicoPosicoes = new List<Vector2>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        SnakeBody corpoParte = collision.gameObject.GetComponent<SnakeBody>();
        if (corpoParte != null && corpoParte.cabeca == this)
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            if (direcaoAtual != Vector2.down)
            {
                proximaDirecao = Vector2.up;
            }
        }
        
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            if (direcaoAtual != Vector2.up)
            {
                proximaDirecao = Vector2.down;
            }
        }
        
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (direcaoAtual != Vector2.right)
            {
                proximaDirecao = Vector2.left;
            }
        }
        
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (direcaoAtual != Vector2.left)
            {
                proximaDirecao = Vector2.right;
            }
        }
    }

    void FixedUpdate()
    {
        direcaoAtual = proximaDirecao;
        
        rb.MovePosition(rb.position + direcaoAtual * velocidade * Time.fixedDeltaTime);

        historicoPosicoes.Insert(0, transform.position);

        int tamanhoNecessario = (corpo.Count * gap) + 5;
        if (historicoPosicoes.Count > tamanhoNecessario)
        {
            historicoPosicoes.RemoveAt(historicoPosicoes.Count - 1);
        }

        int index = 0;
        foreach (Transform parte in corpo)
        {
            index++;
            int posicaoNoHistorico = index * gap;
            
            if (posicaoNoHistorico < historicoPosicoes.Count)
            {
                parte.position = historicoPosicoes[posicaoNoHistorico];
            }
        }
    }

    public void Crescer()
    {
        if (corpoPrefab == null)
        {
            Debug.LogError("corpoPrefab não foi atribuído no Inspector! Por favor, arraste o prefab CorpoCobra para o campo corpoPrefab no componente MovimentoJogador.");
            return;
        }
        
        GameObject novoCorpo = Instantiate(corpoPrefab, transform.position, Quaternion.identity);
        
        SnakeBody bodyScript = novoCorpo.GetComponent<SnakeBody>();
        if(bodyScript != null)
        {
            bodyScript.cabeca = this;
        }
        
        Collider2D corpoCollider = novoCorpo.GetComponent<Collider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (corpoCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, corpoCollider, true);
        }
        
        foreach (Transform parteExistente in corpo)
        {
            Collider2D parteCollider = parteExistente.GetComponent<Collider2D>();
            if (corpoCollider != null && parteCollider != null)
            {
                Physics2D.IgnoreCollision(corpoCollider, parteCollider, true);
            }
        }

        corpo.Add(novoCorpo.transform);
    }

    public int GetTamanho()
    {
        // Tamanho = Cabeça (1) + Corpo
        return 1 + corpo.Count;
    }

    // Retorna TRUE se a cobra ganhou (matou o inimigo), FALSE se perdeu (tomou dano)
    public bool ResolverCombate(int vidaInimigo)
    {
        int meuTamanho = GetTamanho();

        if (meuTamanho > vidaInimigo)
        {
            // GANHOU: Mata o inimigo, mas perde tamanho
            ReduzirTamanho(vidaInimigo);
            Debug.Log("Cobra Venceu! Tamanho reduzido.");
            return true;
        }
        else
        {
            // PERDEU: Inimigo forte demais, toma dano na vida (GameManager)
            // PERDEU: Inimigo forte demais, toma dano na vida (GameManager)
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.ReceberDano();
            }
            else
            {
               Debug.LogError("GameManager não encontrado!");
            }
            Debug.Log("Cobra Perdeu! Dano recebido.");
            return false;
        }
    }

    private void ReduzirTamanho(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            if (corpo.Count > 0)
            {
                // Remove o último pedaço (a ponta da cauda)
                Transform pedaco = corpo[corpo.Count - 1];
                corpo.RemoveAt(corpo.Count - 1);
                Destroy(pedaco.gameObject);
            }
        }
    }
    
    // --- Métodos de Save/Load ---
    
    public List<Vector2> GetHistorico()
    {
        return new List<Vector2>(historicoPosicoes);
    }
    
    public void SetHistorico(List<Vector2> historicoSalvo)
    {
        historicoPosicoes = new List<Vector2>(historicoSalvo);
    }
    
    public void LimparCorpo()
    {
        foreach (Transform parte in corpo)
        {
            if(parte != null) Destroy(parte.gameObject);
        }
        corpo.Clear();
        historicoPosicoes.Clear();
    }
    
    public void ReconstruirCorpo(int tamanhoTotal)
    {
        // O tamanho total inclui a cabeça.
        // Se tamanhoTotal = 5, temos 4 partes de corpo.
        int partesParaAdicionar = tamanhoTotal - 1;
        
        for(int i=0; i < partesParaAdicionar; i++)
        {
            Crescer();
        }
    }
}