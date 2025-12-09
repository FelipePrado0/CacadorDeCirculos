using UnityEngine;
using System.Collections;

public class InimigoPatrulha : MonoBehaviour
{
    public float velocidade = 3f;
    public float distanciaPatrulha = 4f;
    private Vector3 pontoInicial;
    private Vector2 direcaoAtual;
    private float tempoParaMudarDirecao = 0f;
    private float intervaloMudancaDirecao = 2f;

    void Start()
    {
        pontoInicial = transform.position;
        EscolherNovaDirecao();
    }

    void Update()
    {
        tempoParaMudarDirecao -= Time.deltaTime;
        
        if (tempoParaMudarDirecao <= 0f)
        {
            EscolherNovaDirecao();
            tempoParaMudarDirecao = intervaloMudancaDirecao;
        }
        
        Vector2 movimento = direcaoAtual * velocidade * Time.deltaTime;
        transform.Translate(movimento);
        
        float distanciaDoInicio = Vector2.Distance(transform.position, pontoInicial);
        if (distanciaDoInicio >= distanciaPatrulha)
        {
            Vector2 direcaoParaInicio = ((Vector2)pontoInicial - (Vector2)transform.position).normalized;
            direcaoAtual = direcaoParaInicio;
        }
    }
    
    void EscolherNovaDirecao()
    {
        float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        direcaoAtual = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)).normalized;
    }

    public int vidaInimigo = 2; // Vida do inimigo (configurável no inspector)

    private bool jaProcessouColisao = false;
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (jaProcessouColisao) return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            MovimentoJogador jogador = collision.gameObject.GetComponent<MovimentoJogador>();
            if (jogador != null)
            {
                jaProcessouColisao = true;
                bool inimigoMorreu = jogador.ResolverCombate(vidaInimigo);

                if (inimigoMorreu)
                {
                    GameManager gm = FindFirstObjectByType<GameManager>();
                    if (gm != null)
                    {
                        gm.AdicionarPontoInimigo();
                    }
                    Destroy(gameObject);
                }
                else
                {
                    StartCoroutine(ResetarColisao());
                }
            }
        }
        else 
        {
            SnakeBody corpo = collision.gameObject.GetComponent<SnakeBody>();
            if (corpo != null)
            {
                jaProcessouColisao = true;
                GameManager gm = FindFirstObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.ReceberDano();
                }
                StartCoroutine(ResetarColisao());
            }
        }
    }
    
    private System.Collections.IEnumerator ResetarColisao()
    {
        yield return new WaitForSeconds(0.5f);
        jaProcessouColisao = false;
    }
}
