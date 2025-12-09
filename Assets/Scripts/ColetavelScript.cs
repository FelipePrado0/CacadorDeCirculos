using UnityEngine;

public class ColetavelScript : MonoBehaviour
{
    // Esta função é chamada automaticamente pela Unity quando algo entra no trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que colidiu é o "Jogador"
        if (other.gameObject.CompareTag("Player"))
        {
            // Chama a função de adicionar ponto do GameManager
            // Tenta encontrar o GameManager de forma mais segura
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.AdicionarPonto();
            }
            else
            {
                Debug.LogError("GameManager não encontrado pelo Coletável!");
            }
            // Destrói o objeto coletável
            Destroy(gameObject);
        }
    }
}