using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MesaPingPongMapa : MonoBehaviour
{
    [Header("Configurações de Transição")]
    [Tooltip("O nome exato da Scene do teu jogo de Ping Pong (onde a bola e a IA estão)")]
    public string nomeCenaPingPong = "PingPong";

    [Header("Interface Visual")]
    [Tooltip("Arrasta um GameObject (ex: um balãozinho a dizer 'E - Jogar') que vai aparecer quando o jogador estiver perto")]
    public GameObject avisoInteracaoUI;

    private bool jogadorEstaPerto = false;

    void Start()
    {
        // Garante que o aviso está escondido no início
        if (avisoInteracaoUI != null)
        {
            avisoInteracaoUI.SetActive(false);
        }
    }

    void Update()
    {
        // Se o jogador estiver na zona e carregar na tecla "E"
        if (jogadorEstaPerto && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EntrarNoTorneio();
        }
    }

    private void EntrarNoTorneio()
    {
        UnityEngine.Debug.Log("A carregar a partida de Ping Pong...");

        // Carrega a tua cena do jogo!
        SceneManager.LoadScene(nomeCenaPingPong);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem entrou na zona da mesa foi o teu personagem
        // ATENÇÃO: O teu personagem precisa de ter a Tag "Player" no Unity!
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = true;

            // Mostra o aviso flutuante para o jogador saber que pode interagir
            if (avisoInteracaoUI != null)
            {
                avisoInteracaoUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Quando o personagem se afastar da mesa
        if (collision.CompareTag("Player"))
        {
            jogadorEstaPerto = false;

            // Esconde o aviso
            if (avisoInteracaoUI != null)
            {
                avisoInteracaoUI.SetActive(false);
            }
        }
    }
}