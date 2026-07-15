using UnityEngine;
using UnityEngine.SceneManagement; // Permite viajar entre fases

public class GerenciadorDoMenu : MonoBehaviour
{
    public void ClicouEmJogar()
    {
        // Carrega a fase exata. O nome deve ser igualzinho ao da sua cena da fase!
        SceneManager.LoadScene("SampleScene"); 
    }

    public void ClicouEmSair()
    {
        Debug.Log("Fechando o jogo...");
        Application.Quit(); // Só funciona quando compilar o jogo final!
    }
}
