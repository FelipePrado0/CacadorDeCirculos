using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    [Header("Componentes de Áudio")]
    public AudioSource musicaFundo;
    public AudioSource efeitosSonoros;
    
    [Header("Clipes de Áudio")]
    public AudioClip musicaDeFundo;
    public AudioClip somColeta;
    public AudioClip somDano;
    public AudioClip musicaVitoria;
    public AudioClip musicaDerrota;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (musicaFundo == null)
        {
            musicaFundo = gameObject.AddComponent<AudioSource>();
            musicaFundo.loop = true;
            musicaFundo.playOnAwake = false;
        }
        
        if (efeitosSonoros == null)
        {
            efeitosSonoros = gameObject.AddComponent<AudioSource>();
            efeitosSonoros.playOnAwake = false;
        }
    }
    
    void Start()
    {
        if (musicaDeFundo != null && musicaFundo != null)
        {
            musicaFundo.clip = musicaDeFundo;
            musicaFundo.Play();
        }
    }
    
    public void TocarSomColeta()
    {
        if (somColeta != null && efeitosSonoros != null)
        {
            efeitosSonoros.PlayOneShot(somColeta);
        }
    }

    public void TocarSomDano()
    {
        if (somDano != null && efeitosSonoros != null)
        {
            efeitosSonoros.PlayOneShot(somDano);
        }
    }

    public void TocarMusicaVitoria()
    {
        PararMusica();
        if (musicaVitoria != null && efeitosSonoros != null)
        {
            efeitosSonoros.PlayOneShot(musicaVitoria);
        }
    }

    public void TocarMusicaDerrota()
    {
        PararMusica();
        if (musicaDerrota != null && efeitosSonoros != null)
        {
            efeitosSonoros.PlayOneShot(musicaDerrota);
        }
    }
    
    public void PararMusica()
    {
        if (musicaFundo != null)
        {
            musicaFundo.Stop();
        }
    }
    
    public void ContinuarMusica()
    {
        if (musicaFundo != null && !musicaFundo.isPlaying)
        {
            musicaFundo.Play();
        }
    }
}

