using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para poder gerenciar e carregar cenas

public class MenuManager : MonoBehaviour
{
    // Verifica se existe save para habilitar o botão de continuar (opcional visualmente por enquanto)
    private void Start()
    {
        string caminhoArquivo = Application.persistentDataPath + "/savegame.json";
        // Se quiser desabilitar o botão continuar caso não tenha save, faria aqui
    }

    public void IniciarJogo()
    {
        // NOVO JOGO: Define flag para NÃO carregar
        PlayerPrefs.SetInt("CarregarSave", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Jogo");
    }

    public void ContinuarJogo()
    {
        string caminhoArquivo = Application.persistentDataPath + "/savegame.json";
        if (System.IO.File.Exists(caminhoArquivo))
        {
            // CONTINUAR: Define flag para carregar
            PlayerPrefs.SetInt("CarregarSave", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Jogo");
        }
        else
        {
            Debug.Log("Não há jogo salvo para continuar!");
        }
    }

    public void SairDoJogo()
    {
        Debug.Log("Saindo do Jogo...");
        Application.Quit();
    }
    
    public void FecharJogo()
    {
        Debug.Log("Fechando o jogo...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}