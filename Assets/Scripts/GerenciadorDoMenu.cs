using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorDoMenu : MonoBehaviour
{
    public void ClicouMusica1() 
    { 
        SceneManager.LoadScene("FaseMusica1"); 
    }

    public void ClicouMusica2() 
    { 
        SceneManager.LoadScene("FaseMusica2"); 
    }

    public void ClicouMusica3() 
    { 
        SceneManager.LoadScene("FaseMusica3"); 
    }

    public void ClicouEmSair() 
    { 
        Debug.Log("Fechando o jogo...");
        Application.Quit(); 
    }
}